using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Models;
using BackendAPI.Models.User;
using BackendAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _repository;
        private readonly UserManager<User> _userManager;

        public UsersController(IMapper mapper, IUserRepository repository, UserManager<User> userManager)
        {
            _mapper = mapper;
            _repository = repository;
            _userManager = userManager;
        }
        /// <summary>
        /// Obter todos os utilizadores no sistema
        /// </summary>
        /// <remarks>
        /// Obtém todos os utilizadorres no sistema. 
        /// 
        /// Este endpoint apenas está disponível para administradores, e devolve todos os detalhes dos utilizadores, como se fosse o utilizador a ver o seu próprio perfil
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Lista com todos os utilizadores no sistema</response>
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers()
        {
            IEnumerable<User> users = await _repository.GetAll();
            IEnumerable<UserModelSelf> users_ret = from user in users select _mapper.Map<User, UserModelSelf>(user);
            return Ok(users_ret);
        }
        /// <summary>
        /// Obtém um utilizador no sistema pelo seu ID
        /// </summary>
        /// <remarks>
        /// Consideremos "A" o utilizador que realiza este pedido e "B" o utilizador a consultar.
        /// 
        /// Obtém os detalhes de B pelo seu ID. Se A for um administrador, tem acesso a todos os detalhes de B.
        /// 
        /// Para uma interface a este API, isto é útil, visto que se atributos como "convites" ou "email" de B estiverem presentes, significa que há permissão para actualizar os detalhes de B.
        /// 
        /// Caso A não seja administrador do sistema, o mesmo não poderá ver:
        /// 
        /// Os grupos a que B pertence, caso os mesmos sejam privados e o A não pertença aos mesmos.
        /// 
        /// As viagens a que B pertence, caso as mesmas sejam privadas e A não esteja nas mesmas, ou A não pertença 
        /// ao grupo em que elas se situam, caso o grupo seja privado.
        /// 
        /// As publicações que o B realizou, caso as mesmas sejam privadas, as viagens onde as publicações foram feitas sejam privadas e A não pertença às mesmas, 
        /// ou o o grupo onde estas viagens estão situadas seja privado e A não pertença a este grupo.
        /// 
        /// B tem também uma lista de pessoas que segue e uma lista de pessoas que o seguem. Se A não for administrador, apenas consegue ver em ambos contas activas.
        /// </remarks>
        /// <param name="id">ID do utilizador</param>
        /// <returns></returns>
        /// <response code="200">Modelo JSON com os detalhes do utilizador, com mais detalhes caso quem realize o pedido seja um administrador do sistema</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID, ou o mesmo está desactivado, caso o utilizador que realize o pedido não seja administrador do sistema</response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserById(String id)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            User user = await _repository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }
            if (!is_admin)
            {
                if (!user.IsActive)
                {
                    return NotFound();
                }
                //Esconder Grupos privados em que o utilizador atual não esteja presente
                user.Groups = user.Groups.Where(ug =>
                !(ug.Group.IsPrivate &&
                !current_user.Groups.Any(gg => gg.Group.Id == ug.Group.Id))).ToList();
                //Esconder Trips e Posts de Trips que estejam escondidas e o utilizador não pertença à mesma, ou que pertençam a grupos privados que o utilizador actual não esteja presente
                user.Trips = user.Trips.Where(ut =>
                !(ut.Trip.IsPrivate && !current_user.Trips.Any(cut => cut.Trip.Id == ut.Trip.Id)) &&
                !(ut.Trip.Group.IsPrivate && !current_user.Groups.Any(gg => gg.Group.Id == ut.Trip.Group.Id))).ToList();
                user.Posts = user.Posts.Where(up =>
                !(up.Trip.IsPrivate && !current_user.Trips.Any(cut => cut.Trip.Id == up.Trip.Id)) &&
                !(up.Trip.Group.IsPrivate && !current_user.Groups.Any(gg => gg.Group.Id == up.Trip.Group.Id)) &&
                !up.IsHidden).ToList();
                user.Following = user.Following.Where(f => f.IsActive).ToList();
                user.Followers = user.Followers.Where(f => f.IsActive).ToList();
                return Ok(_mapper.Map<User, UserModel>(user));
            }
            else
            {
                return Ok(_mapper.Map<User, UserModelSelf>(user));
            }
        }
        /// <summary>
        /// Obter os detalhes da própria conta
        /// </summary>
        /// <remarks>
        /// Obtém os detalhes da conta do próprio utilizador que realiza este pedido.
        /// 
        /// Se o utilizador que realiza este pedido não for um administrador do sistema, o mesmo não irá conseguir ver publicações que realizou que foram "eliminadas" e não conseguirá ver membros da plataforma que seguia ou
        /// que o seguiam que tenham tido as suas contas desactivadas.
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Modelo JSON com os detalhes do próprio perfil</response>
        [Authorize]
        [HttpGet("Self")]
        public async Task<ActionResult<UserModelSelf>> GetOwnUser()
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            if (is_admin)
            {
                return Ok(_mapper.Map<User, UserModelSelf>(current_user));
            }
            current_user.Posts = current_user.Posts.Where(up => !up.IsHidden).ToList();
            current_user.Following = current_user.Following.Where(f => f.IsActive).ToList();
            current_user.Followers = current_user.Followers.Where(f => f.IsActive).ToList();
            return Ok(_mapper.Map<User, UserModelSelf>(current_user));
        }
        /// <summary>
        /// Actualizar os detalhes de um utilizador pelo seu ID
        /// </summary>
        /// <remarks>
        /// Apenas um administrador do sistema pode actualizar os detalhes de outro utilizador. Para um utilizador actualizar os detalhes da sua própria conta, tem que usar o endpoint PUT /Users/Self
        /// 
        /// Num frontend, é possível saber se se pode usar este endpoint, se no endpoint GET /Users/Id se recebem atributos como o e-mail ou convites para grupos/viagens, mesmo que essas listas estejam vazias.
        /// </remarks>
        /// <param name="Id">ID do utilizador a actualizar</param>
        /// <param name="userDetailsUpdateModel">Modelo JSON com os novos detalhes do utilizador a actualizar</param>
        /// <returns></returns>
        /// <response code="200">Detalhes do utilizador actualizados com sucesso</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID</response>
        /// <response code="400">Caso o código de país ISO com 2 caracteres seja inválido ou caso se tenha pretendido actualizar o e-mail, o envio do e-mail de confirmação de e-mail não foi concretizado.</response>
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserDetails(string Id, UserDetailsUpdateModel userDetailsUpdateModel)
        {
            User user = await _repository.GetById(Id);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.Update(user, userDetailsUpdateModel);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Actualizar ou Inserir a fotografia de perfil de um utilizador no sistema
        /// </summary>
        /// <remarks>
        /// Apenas o administrador pode utilizar este endpoint, tal como apenas o mesmo pode utilizar o endpoint de actualizar detalhes de outros utilizadores.
        /// 
        /// Como não é possível passar ficheiros em modelos JSON, têm-se este endpoint http que recebe um ficheiro de um formulário e o ID de um utilizador, e actualiza a sua fotografia, caso já tenha uma existente,
        /// ou insere uma nova e associa ao utilizador em questão.
        /// </remarks>
        /// <param name="userId">ID do utilizador a inserir ou actualizar a fotografia de perfil</param>
        /// <param name="file">Ficheiro JPG, JPEG, PNG ou GIF contendo a imagem do utilizador.</param>
        /// <returns></returns>
        /// <response code="200">Fotografia do utilizador inserida ou actualizada com sucesso</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID</response>
        /// <response code="400">Caso a imagem não esteja num formato válido ou tenha ocorrido um erro ao dar upload da imagem para a Google Cloud Storage.</response>
        [Authorize(Roles = "ADMIN")]
        [HttpPost("{UserId}/Picture")]
        public async Task<ActionResult> UpdateUserPicture(String userId, [FromForm] IFormFile file)
        {
            User user = await _repository.GetById(userId);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.UpdateImage(user, file);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Remover a imagem de perfil de um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador a remover a imagem de perfil</param>
        /// <returns></returns>
        /// <response code="200">Fotografia do utilizador removida com sucesso</response>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{UserId}/Picture")]
        public async Task<ActionResult> DeleteUserPicture(String userId)
        {
            User user = await _repository.GetById(userId);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                await _repository.DeleteImage(user);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Actualizar os detalhes do próprio utilizador
        /// </summary>
        /// <param name="userDetailsUpdateModel">Modelo JSON com os detalhes da conta a actualizar</param>
        /// <returns></returns>
        /// <response code="200">Detalhes do utilizador actualizados com sucesso</response>
        /// <response code="400">Caso o código de país ISO com 2 caracteres seja inválido ou caso se tenha pretendido actualizar o e-mail, o envio do e-mail de confirmação de e-mail não foi concretizado.</response>
        [Authorize]
        [HttpPut("Self")]
        public async Task<IActionResult> UpdateUserDetailsSelf(UserDetailsUpdateModel userDetailsUpdateModel)
        {
            User user = await _userManager.GetUserAsync(this.User);
            try
            {
                await _repository.Update(user, userDetailsUpdateModel);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Actualizar ou Inserir a própria fotografia de perfil
        /// </summary>
        /// <param name="file">Ficheiro JPG, JPEG, PNG ou GIF contendo a imagem do utilizador.</param>
        /// <returns></returns>
        /// <response code="200">Fotografia do utilizador inserida ou actualizada com sucesso</response>
        /// <response code="400">Caso a imagem não esteja num formato válido ou tenha ocorrido um erro ao dar upload da imagem para a Google Cloud Storage.</response>
        [Authorize]
        [HttpPost("Self/Picture")]
        public async Task<ActionResult> UpdateOwnPicture([FromForm] IFormFile file)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            try
            {
                await _repository.UpdateImage(current_user, file);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Remover a própria imagem de perfil
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Fotografia do utilizador removida com sucesso</response>
        [Authorize]
        [HttpDelete("Self/Picture")]
        public async Task<ActionResult> DeleteOwnPicture()
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            try
            {
                await _repository.DeleteImage(current_user);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Remover um utilizador do sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint remove um utilizador no sistema.
        /// 
        /// O utilizador irá sair de todas as viagens, grupos, irá ter todos os seus convites para viagens e grupos removidos, a sua imagem removida, todas as suas publicações e anexos removidos e a sua conta irá ser
        /// permanentemente removida.
        /// 
        /// Não poderá ser reactivado.
        /// 
        /// Apenas o administrador poderá aceder a este endpoint, sendo que os utilizadores apenas podem "apagar" a sua conta através de actualizarem os detalhes da sua conta e definirem "IsActive" como falso.
        /// Isso irá igualmente desactivar a sua conta, mas será mais fácil para um administrador a reactivar, e não remove verdadeiramente o utilizador do sistema.
        /// </remarks>
        /// <param name="Id">ID do utilizador a remover</param>
        /// <returns></returns>
        /// <response code="200">Remoção do Utilizador do Sistema bem-sucedida</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID</response>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(String Id)
        {
            User user = await _repository.GetById(Id);
            if (user == null)
            {
                return NotFound();
            }
            await _repository.Delete(user);
            return Ok();
        }
        /// <summary>
        /// Pesquisar por utilizadores no sistema
        /// </summary>
        /// <remarks>
        /// Como no endpoint de obter um utilizador por ID, os administradores do sistema conseguem ver todos os detalhes dos utilizadores, e conseguem obter detalhes sobre utilizadores não-activos.
        /// 
        /// Irá ser utilizado para pesquisar utilizadores no sistema na página de utilizadores e pesquisar utilizadores para convidar para um grupo.
        /// </remarks>
        /// <param name="userSearchModel">Modelo de pesquisa de utilizadores, que permite pesquisar um utilizador pelo seu e-mail ou nome</param>
        /// <returns></returns>
        /// <response code="200">"Lista de resultados da pesquisa de utilizadores com os parâmetros utilizados</response>
        [Authorize]
        [HttpPost("Search")]
        public async Task<ActionResult<IEnumerable<UserModelSimple>>> SearchUser(UserSearchModel userSearchModel)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            IList<string> user_roles = await _userManager.GetRolesAsync(current_user);
            bool is_admin = user_roles.Contains(Enum.GetName(UserRole.ADMIN));
            IEnumerable<User> users = await _repository.Search(userSearchModel);
            //Admins conseguem ver todos os utilizadores e todos os seus detalhes, mesmo que não partilhem viagens/grupos
            if (!is_admin)
            {
                //Excluir o próprio e utilizadores inactivos
                users = users.Where(u => u.IsActive && u.Id != current_user.Id);
                IEnumerable<UserModelSimple> users_ret = from user in users select _mapper.Map<User, UserModelSimple>(user);
                return Ok(users_ret);
            }
            else
            {
                IEnumerable<UserModelSelf> users_ret = from user in users select _mapper.Map<User, UserModelSelf>(user);
                return Ok(users_ret);
            }
        }
        /// <summary>
        /// Actualizar os roles/permissões/papéis do utilizador no sistema
        /// </summary>
        /// <remarks>
        /// Permite actualizar os papéis/permissões de um uitilizador no sistema.
        /// 
        /// Contém a lista de IDs de papéis num array JSON, correspondendo a um enum bem-definido (0 para Admin, 1 para Utilizador normal, futuramente mais para outros papéis relevantes
        /// 
        /// Com este endpoint pode-se remover ou adicionar um utilizador a papéis.
        /// </remarks>
        /// <param name="userId">ID do utilizador a actualizar os papéis</param>
        /// <param name="model">Modelo JSON com uma lista de papéis de utilizador no sistema (de momento apenas administrador e utilizador regular)</param>
        /// <returns></returns>
        /// <response code="200">Actualização dos papéis do utilizador bem-sucedida</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID</response>
        /// <response code="400">Role passado não existe</response>

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{userId}/Roles")]
        public async Task<ActionResult> UpdateUserRoles(String userId, UpdateUserRoleModel model)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            User user = await _repository.GetById(userId);
            if (user == null)
            {
                return NotFound();
            }
            //prevenir ficar sem admins
            if (current_user == user)
            {
                return BadRequest("An admin can't update his own roles");
            }
            try
            {
                await _repository.UpdateUserRoles(user, model.Roles);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Seguir um utilizador
        /// </summary>
        /// <param name="id">ID do Utilizador a seguir</param>
        /// <returns></returns>
        /// <response code="200">Utilizador seguido com sucesso</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID</response>
        /// <response code="400">Utilizador que realizou este pedido já segue o utilizador especificado</response>
        [Authorize]
        [HttpPut("Following/{id}")]
        public async Task<ActionResult> FollowUser(String id)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            User user = await _repository.GetById(id);
            if (user == null || !user.IsActive)
            {
                return NotFound();
            }
            try
            {
                await _repository.FollowUser(current_user, user);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
        /// <summary>
        /// Deixar de seguir um utilizador
        /// </summary>
        /// <param name="id">ID do Utilizador a deixar de seguir</param>
        /// <returns></returns>
        /// <response code="200">Remoção do seguimento do utilizador com sucesso</response>
        /// <response code="404">Não existe um utilizador no sistema com este ID</response>
        /// <response code="400">Utilizador que realizou este pedido não seguia o utilizador especificado</response>
        [Authorize]
        [HttpDelete("Following/{id}")]
        public async Task<ActionResult> UnfollowUser(String id)
        {
            User current_user = await _userManager.GetUserAsync(this.User);
            User user = await _repository.GetById(id);
            if (user == null || !user.IsActive)
            {
                return NotFound();
            }
            try
            {
                await _repository.UnfollowUser(current_user, user);
                return Ok();
            }
            catch (CustomException exception)
            {
                return BadRequest(new ErrorModel() { ErrorType = exception.ErrorType, Message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorModel() { ErrorType = ErrorType.OTHER, Message = ex.Message });
            }
        }
    }
}
