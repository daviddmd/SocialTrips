using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Helpers;
using BackendAPI.Models;
using BackendAPI.Models.Authentication;
using BackendAPI.Models.User;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<User> signInManager;
        private readonly IConfiguration _configuration;
        private readonly IRankingRepository rankingRepository;
        private readonly IEmailHelper _emailHelper;
        private readonly string _domain;
        public AuthenticationController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IConfiguration configuration, IRankingRepository rankingRepository, IEmailHelper emailHelper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            _configuration = configuration;
            this.rankingRepository = rankingRepository;
            _emailHelper = emailHelper;
            _domain = configuration["Domain"];
        }
        /// <summary>
        /// Registar um utilizador na plataforma
        /// </summary>
        /// <remarks>
        /// Este endpoint regista um utilizador na plataforma Viagens Sociais.
        /// 
        /// Requer o seu e-mail, password, nome de utilizador pretendido, nome, código ISO de dois caracteres do seu país, e locale (linguagem/localização preferida).
        /// 
        /// Opcionalmente, recebe a sua descrição e número de telemóvel
        /// 
        /// O primeiro utilizador a criar conta é automaticamente um administrador do sistema
        /// </remarks>
        /// <param name="model">Modelo do JSON com todos os detalhes relevantes do utilizador a registar</param>
        /// <returns></returns>
        /// <response code="200">Criação do Utilizador bem-sucedida</response>
        /// <response code="400">
        /// Pode acontecer em vários casos:
        /// 
        /// Existe um utilizador com o mesmo e-mail no sistema
        /// 
        /// Existe um utilizador com o mesmo nome de utilizador no sistema
        /// 
        /// A password não satisfaz as necessidades de segurança do sistema
        /// 
        /// Código ISO de 2 caracteres do país inválido
        /// 
        /// Falha ao enviar o e-mail de confirmação de conta
        /// </response>
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserRegisterModel model)
        {
            //create default roles
            foreach (string role in Enum.GetNames(typeof(UserRole)))
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            Ranking ranking = await rankingRepository.GetDefaultRanking();
            if (ranking == null)
            {
                ranking = await rankingRepository.CreateDefaultRanking();
            }
            User userNameExists = await userManager.FindByNameAsync(model.UserName);
            User emailExists = await userManager.FindByEmailAsync(model.Email);
            if (userNameExists != null)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_USERNAME_EXISTS, Message = "An account with this username already exists" });
            }
            if (emailExists != null)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_EMAIL_EXISTS, Message = "An account with this email already exists" });

            }
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_INVALID_PASSWORD, Message = "Passwords don't match" });
            }

            try
            {
                RegionInfo info = new(model.Country);
                //eventualmente usar isto para mais coisas, como formato de horas, moeda, etc. De momento apenas para validar se é um nome de país válido (ISO)
            }
            catch (ArgumentException)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.INVALID_COUNTRY_CODE, Message = "A country with this code doesn't exist" });
            }
            int NumberUsers = await userManager.Users.CountAsync();
            User user = new()
            {
                Email = model.Email,
                UserName = model.UserName,
                Name = model.Name,
                Country = model.Country,
                City = model.City,
                PhoneNumber = model.PhoneNumber,
                Description = model.Description,
                Ranking = ranking,
                Locale = model.Locale,
                CreationDate = DateTime.Now,
                IsActive = true,
                Facebook = "",
                Twitter = "",
                Instagram = ""
            };
            IdentityResult result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_ACCOUNT_CREATION_ERROR, Message = $"Failure Creating the Account: {result.ToString()}" });
            }
            await userManager.AddToRoleAsync(user, Enum.GetName(UserRole.USER));
            if (NumberUsers == 0)
            {
                await userManager.AddToRoleAsync(user, Enum.GetName(UserRole.ADMIN));
            }
            if (userManager.Options.SignIn.RequireConfirmedAccount)
            {
                String EmailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                string EmailConfirmationLink = _emailHelper.GetEmailConfirmationLink(user.Id, EmailConfirmationToken);
                string body = $@"
                <h3>Bem Vindo à Plataforma das Viagens Sociais!</h3><br>
                <p>Estamos muito gratos pelo teu registo e não podemos esperar pelas tuas contribuições para a nossa grande comunidade de viajantes!</p>
                <p>Para completar o teu registo, por favor, clica <a href='{EmailConfirmationLink}'>neste link</a> ou acede ao link abaixo e começa já a viajar!</p>
                <p>{EmailConfirmationLink}</p>
                <p>Estamos à tua espera!</p>";
                bool MailResult = await _emailHelper.SendEmail("Bem-Vindo à Plataforma das Viagens Sociais!", body, user.Email);
                if (!MailResult)
                {
                    return BadRequest(new ErrorModel() { ErrorType = ErrorType.EMAIL_ERROR, Message = "The e-mail couldn't be delivered" });
                }
            }
            return Ok();
        }
        /// <summary>
        /// Autenticar o utilizador no sistema
        /// </summary>
        /// <param name="model">Modelo JSON com os detalhes de autenticação do utilizador, nomeadamente o seu nome de utilizador/email, a sua password e a opção do token "não expirar" (continuar autenticado "indefinidamente")</param>
        /// <returns></returns>
        /// <response code="200">
        /// Autenticação do Utilizador bem sucedida, com o token e data de expiração do token
        /// 
        ///     {
        ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.TOKEN.PpfMuskowxR-8riVs25xlacoTjnSMaVj7PHDKobkqUQ",
        ///         "expiration": "2023-01-06T11:59:32Z"
        ///     }
        /// </response>
        /// <response code="401">
        /// Pode acontecer em vários casos:
        /// 
        /// A password para este utilizador está incorrecta.
        /// 
        /// A conta do utilizador não está confirmada, ou está bloqueada.
        /// </response>
        /// <response code ="400">
        /// Pode acontecer em vários casos:
        /// 
        /// Não existe um utilizador com um nome de utilizador ou e-mail especificados
        /// 
        /// O utilizador em questão está desactivado.
        /// </response>
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            User user = await userManager.FindByNameAsync(model.EmailOrUserName) ?? await userManager.FindByEmailAsync(model.EmailOrUserName);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_INVALID_USER, Message = "Invalid username or email" });
            }
            if (!await userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_WRONG_PASSWORD, Message = "Username or password wrong" });
            }
            if (!await signInManager.CanSignInAsync(user))
            {
                return Unauthorized(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_ACCOUNT_NOT_CONFIRMED, Message = "Account not confirmed" });
            }
            IList<string> userRoles = await userManager.GetRolesAsync(user);
            List<Claim> authClaims = new()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.SerialNumber, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            foreach (string userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            JwtSecurityToken token = new(
                claims: authClaims,
                expires: (model.RememberMe) ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
        /// <summary>
        /// Enviar um E-Mail de Redfinição de Password para o Utilizador
        /// </summary>
        /// <param name="model">Modelo JSON para pedir a requisição de password da conta de um utilizador. Contém o nome de utilizador ou email da conta</param>
        /// <returns></returns>
        /// <response code="200">E-Mail com link de re-definição de password enviado para o e-mail do utilizador</response>
        /// <response code="400">
        /// Pode acontecer em vários casos:
        /// 
        /// Não existe um utilizador com um nome de utilizador ou e-mail especificados
        /// 
        /// O utilizador em questão está desactivado.
        /// 
        /// O E-Mail de re-definição de password não pode ser entregue.
        /// 
        /// Conta do utilizador não está confirmada
        /// </response>
        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            User user = await userManager.FindByNameAsync(model.EmailOrUsername) ?? await userManager.FindByEmailAsync(model.EmailOrUsername);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_INVALID_USER, Message = "Invalid username or email" });
            }
            if (!await signInManager.CanSignInAsync(user))
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_ACCOUNT_NOT_CONFIRMED, Message = "Account not confirmed" });
            }
            string PasswordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            string PasswordResetLink = _emailHelper.GetPasswordResetLink(user.Id, PasswordResetToken);
            string body = @$"
            <h5>Link de Recuperação da Password na plataforma de Viagens Sociais</h5>
            <p>Foi requisitado um link de recuperação de password para a plataforma das redes sociais. Clique <a href='{PasswordResetLink}'>aqui</a> ou aceda ao link abaixo para redifinir a sua password</p>
            <p>{PasswordResetLink}</p>
            <p>Se não requisitou esta operação, por favor ignore este e-mail.</p>";
            bool MailResult = await _emailHelper.SendEmail("Recuperar Password Viagens Sociais", body, user.Email);
            if (!MailResult)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.EMAIL_ERROR, Message = "The e-mail couldn't be delivered" });
            }
            return Ok();
        }
        /// <summary>
        /// Re-definir a password do utilizador
        /// </summary>
        /// <remarks>
        /// Re-define a password do utilizador.
        /// 
        /// O modo como este endpoint é suposto ser utilizado, num contexto web pelo menos, será:
        /// 
        /// Após o utilizador clicar no link de confirmação enviado, como por exemplo, https://viagens-sociais.xyz/reset-password/{userId}/{PasswordConfirmationTokenEncoded} , em que o token de confirmação está em base64
        /// O mesmo irá ser deparado com uma página normal de re-definição de password. Porém, esta página não sabe se o token é válido, apenas cria um formulário com o User ID e token de re-definição, agora na sua forma real
        /// após ser descodificado de base64, escondidos. O utilizador irá escrever a sua nova password (e confirmá-la), e depois submeter esse formulário, com o ID do utilizador, token de confirmação na sua forma original e 
        /// a nova password. Quando este modelo JSON for recebido, o servidor irá verificar se um utilizador com este ID existe, e se o token de re-definição de password é válido. Se forem, a password é re-definida e uma resposta
        /// 200 gerada. Com esta resposta, a página de re-definição de password sabe que a re-definição foi bem sucedida, e re-direcciona o utilizador para a página de login, com um aviso que a re-definição foi bem-sucedida.
        /// Caso contrário, avisa o utilizador do erro em questão a partir do ID gerado (ou da mensagem genérica).
        /// </remarks>
        /// <param name="model">Modelo JSON com o ID do utilizaor, token de confirmação de re-definição de password e nova password</param>
        /// <returns></returns>
        /// <response code="200">Password re-definida com sucesso</response>
        /// <response code="400">
        /// Pode acontecer em vários casos:
        /// 
        /// Não existe um utilizador com um nome de utilizador ou e-mail especificados
        /// 
        /// O utilizador em questão está desactivado.
        /// 
        /// O E-Mail de aviso de re-definição de password não pode ser entregue.
        /// </response>
        /// <response code="401">O token de re-definição de password é inválido para o utilizador em questão</response>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            User user = await userManager.FindByIdAsync(model.UserId);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_INVALID_USER, Message = "Invalid username or email" });
            }
            IdentityResult result = await userManager.ResetPasswordAsync(user, model.PasswordResetToken, model.NewPassword);
            if (!result.Succeeded)
            {
                return Unauthorized(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_WRONG_PASSWORD_RESET, Message = "Wrong password reset token" });
            }
            string body = @$"
            <h5>A sua password foi redifinida com sucesso!</h5><br>
            <p>Pode agora fazer login <a href='{_domain}/login'>aqui</a> com as suas novas credenciais.</p>
            <p>Se não realizou esta operação, por favor mude a sua password do seu e-mail ({user.Email}) visto que há uma possibilidade de ter sido comprometido.</p>";
            bool MailResult = await _emailHelper.SendEmail("Password Redifinida na plataforma Viagens Sociais", body, user.Email);
            if (!MailResult)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.EMAIL_ERROR, Message = "The e-mail couldn't be delivered" });
            }
            return Ok();
        }
        /// <summary>
        /// Confirmar o E-Mail do Utilizador
        /// </summary>
        /// <remarks>
        /// Como o endpoint de re-definição de password, a confirmação de conta/e-mail irá funcionar do mesmo modo. O link de confirmação de e-mail é enviado por e-mail e consiste num URL com o ID do utilizador e 
        /// token de confirmação de e-mail codificado em base64. O frontend ao carregar este URL irá fazer um pedido a este endpoint, e dependendo da resposta irá redireccionar o utilizador para a página de autenticação, ou 
        /// apresentar uma mensagem de erro.
        /// </remarks>
        /// <param name="model">Modelo JSON com o ID do utilizador e o token de confirmação de e-mail</param>
        /// <returns></returns>
        /// <response code="200">E-Mail confirmado com sucesso</response>
        /// <response code="400">
        /// Pode acontecer em vários casos:
        /// 
        /// Não existe um utilizador com um nome de utilizador ou e-mail especificados
        /// 
        /// O utilizador em questão está desactivado.
        /// 
        /// O E-Mail de aviso de confirmação do e-mail não pode ser entregue.
        /// </response>
        /// <response code="401">O token de confirmação do e-mail é inválido para o utilizador em questão</response>
        [AllowAnonymous]
        [HttpPost]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
        {
            User user = await userManager.FindByIdAsync(model.UserId);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_INVALID_USER, Message = "Invalid username or email" });
            }
            if (user.EmailConfirmed)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_EMAIL_ALREADY_CONFIRMED, Message = "Email is already confirmed" });

            }
            IdentityResult result = await userManager.ConfirmEmailAsync(user, model.EmailConfirmationToken);
            if (!result.Succeeded)
            {
                return Unauthorized(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_WRONG_EMAIL_CONFIRMATION, Message = "Wrong email confirmation token" });
            }
            string body = @$"
            <h5>O seu E-Mail foi confirmado com sucesso!</h5><br>
            <p>Pode agora fazer login <a href='{_domain}/login'>aqui</a> com as suas credenciais e disfrutar de tudo o que a nossa plataforma tem para oferecer!</p>
            <p>Se tiver alguma dificuldade neste processo, contacte-nos nos contactos na nossa <a href='{_domain}'>Página</a>.</p>";
            bool MailResult = await _emailHelper.SendEmail("E-Mail confirmado na plataforma viagens sociais", body, user.Email);
            if (!MailResult)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.EMAIL_ERROR, Message = "The e-mail couldn't be delivered" });
            }
            return Ok();
        }
        /// <summary>
        /// (Re-)Enviar o E-Mail de Confirmação
        /// </summary>
        /// <param name="model">Modelo JSON para pedir a confirmação do e-mail conta de um utilizador. Contém o nome de utilizador ou email da conta</param>
        /// <returns></returns>
        /// <response code="200">E-Mail com link de re-definição de password enviado para o e-mail do utilizador</response>
        /// <response code="400">
        /// Pode acontecer em vários casos:
        /// 
        /// Não existe um utilizador com um nome de utilizador ou e-mail especificados
        /// 
        /// O utilizador em questão está desactivado.
        /// 
        /// O E-Mail de re-definição de password não pode ser entregue.
        /// 
        /// E-Mail do Utilizador já está confirmado
        /// </response>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResendEmailConfirmation")]
        public async Task<IActionResult> SendConfirmEmail(ResendEmailConfirmationModel model)
        {
            User user = await userManager.FindByNameAsync(model.EmailOrUsername) ?? await userManager.FindByEmailAsync(model.EmailOrUsername);
            if (user == null || !user.IsActive)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_INVALID_USER, Message = "Invalid username or email" });
            }
            if (user.EmailConfirmed)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.AUTHENTICATION_EMAIL_ALREADY_CONFIRMED, Message = "Email is already confirmed" });
            }
            string EmailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string EmailConfirmationLink = _emailHelper.GetEmailConfirmationLink(user.Id, EmailConfirmationToken);
            string body = @$"
            <h5>Para confirmar o seu e-mail, clique <a href='{EmailConfirmationLink}'>aqui</a> ou aceda ao link abaixo</h5>
            <p>{EmailConfirmationLink}</p>
            <p>Se tiver alguma dificuldade neste processo, contacte-nos nos contactos na nossa <a href='{_domain}'>Página</a>.</p>";

            bool MailResult = await _emailHelper.SendEmail("Link de Confirmação de E-Mail na plataforma Viagens Sociais", body, user.Email);
            if (!MailResult)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.EMAIL_ERROR, Message = "The e-mail couldn't be delivered" });
            }
            return Ok();

        }
    }
}
