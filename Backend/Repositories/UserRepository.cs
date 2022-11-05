using BackendAPI.Data;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private string GetPasswordResetLink(string userId, string passwordConfirmationToken)
        {
            String PasswordConfirmationTokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordConfirmationToken));
            return $"{_domain}/reset-password/{userId}/{PasswordConfirmationTokenEncoded}";
        }
        private readonly DatabaseContext _context;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly string _domain;
        private readonly IGoogleCloudStorageHelper _storageHelper;
        private readonly IEmailHelper _emailHelper;

        public UserRepository(DatabaseContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IGoogleCloudStorageHelper storageHelper, IEmailHelper emailHelper)
        {
            _context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _storageHelper = storageHelper;
            _emailHelper = emailHelper;
            _domain = configuration["Domain"];
        }
        public async Task Delete(User user)
        {
            user.GroupInvites.Clear();
            user.TripInvites.Clear();
            user.Followers.Clear();
            user.Following.Clear();
            user.Groups.Clear();
            user.Trips.Clear();
            foreach(Post post in user.Posts)
            {
                foreach(Attachment attachment in post.Attachments)
                {
                    await _storageHelper.Delete(attachment.StorageName);
                }
                post.Attachments.Clear();
            }
            user.Posts.Clear();
            if (user.Photo != null)
            {
                await _storageHelper.Delete(user.Photo.StorageName);
            }
            await userManager.UpdateAsync(user);
            IdentityResult result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                string body = @$"
                <p>A sua conta na plataforma Viagens Sociais foi eliminada com sucesso.</p>
                <p>Se alguma vez desejar voltar, crie uma nova conta que as nossas portas estão sempre abertas!";
                await _emailHelper.SendEmail("Conta na plataforma Viagens Sociais eliminada", body, user.Email);
            }
        }
        public async Task<IEnumerable<User>> GetAll()
        {
            return await userManager.Users.ToListAsync();
        }

        public async Task<User> GetById(String Id)
        {
            return await userManager.FindByIdAsync(Id);
        }

        public async Task<IEnumerable<User>> Search(UserSearchModel userSearch)
        {
            if (userSearch.NameOrEmail == "")
            {
                return new List<User>();
            }
            return await userManager.Users.Where(user => 
            user.Name.ToLower().Contains(userSearch.NameOrEmail.ToLower()) ||
            user.Email.ToLower() == userSearch.NameOrEmail.ToLower() || 
            user.UserName.ToLower().Contains(userSearch.NameOrEmail.ToLower())
            ).ToListAsync();
        }
        //não aparenta haver outro modo de fazer isto infelizmente
        public async Task<IdentityResult> Update(User user, UserDetailsUpdateModel model)
        {
            try
            {
                RegionInfo info = new(model.Country);
            }
            catch (ArgumentException)
            {
                throw new CustomException("A country with this code doesn't exist", ErrorType.INVALID_COUNTRY_CODE);
            }
            user.Name = model.Name;
            user.Country = model.Country;
            user.City = model.City;
            user.Description = model.Description;
            user.Facebook = model.Facebook;
            user.Instagram = model.Instagram;
            user.Twitter = model.Twitter;
            user.PhoneNumber = model.PhoneNumber;
            //Limpar os convites de grupos e viagens do utilizador, mas deixar as suas publicações, grupos e viagens a que pertence como estão
            if (!model.IsActive && user.IsActive)
            {
                user.TripInvites.Clear();
                user.GroupInvites.Clear();
                string Body = $@"
                Olá. Em resposta ao seu pedido de desactivação de conta, a sua conta foi desactivada com sucesso.
                <p>Se a pretender reactivar, por favor contacte a nossa equipa de apoio pelo nosso e-mail no final do nosso website ou nas nossas redes sociais.</p>
                <p>Com os melhores cumprimentos</p>";
                await _emailHelper.SendEmail("Conta Viagens Sociais desactivada", Body, user.Email);
            }
            if (model.IsActive && !user.IsActive)
            {
                string Body = $@"
                Olá. Em resposta ao seu pedido de reactivação da conta, enviamos-lhe um link para re-definir a sua password.
                <p>Pode agora fazer login <a href='{_domain}/login'>aqui</a> e voltar a disfrutar da comunidade.</p>
                Com os melhores cumprimentos";
                await _emailHelper.SendEmail("Conta Viagens Sociais reactivada", Body, user.Email);
            }
            user.IsActive = model.IsActive;
            user.Locale = model.Locale;
            if (model.Email != user.Email)
            {
                string EmailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                string EmailConfirmationLink = _emailHelper.GetEmailConfirmationLink(user.Id, EmailConfirmationToken);
                string body = @$"
                <h5>Para confirmar o seu novo e-mail na plataforma Viagens Sociais, clique <a href='{EmailConfirmationLink}'>aqui</a> ou aceda ao link abaixo</h5>
                <p>{EmailConfirmationLink}</p>
                <p>Se tiver alguma dificuldade neste processo, contacte-nos nos contactos na nossa <a href='{_domain}'>Página</a>.</p>";
                bool MailResult = await _emailHelper.SendEmail("Confirmação do novo endereço de e-mail da plataforma Viagens Sociais", body, model.Email);
                if (!MailResult)
                {
                    throw new CustomException("The e-mail couldn't be delivered", ErrorType.EMAIL_ERROR);
                }
                user.EmailConfirmed = false;
                user.Email = model.Email;

            }
            return await userManager.UpdateAsync(user);
        }
        public async Task UpdateImage(User user, IFormFile file)
        {
            if (file != null && FileHelper.IsImage(file))
            {
                //delete the old one if exists
                await DeleteImage(user);
                String OriginalFileName = file.FileName;
                String DestinationFileName = $"user_{user.Id}{Path.GetExtension(file.FileName).ToLower()}";
                String Url = await _storageHelper.Upload(file, DestinationFileName);
                Attachment attachment = new() { OriginalFileName = OriginalFileName, StorageName = DestinationFileName, Url = Url, UploadedDate = DateTime.Now };
                _context.Attachments.Add(attachment);
                user.Photo = attachment;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("No valid image passed", ErrorType.MEDIA_ERROR);
            }

        }

        public async Task UpdateUserRoles(User user, List<UserRole> roles)
        {
            foreach(UserRole role in roles)
            {
                if (!Enum.IsDefined(role))
                {
                    throw new CustomException($"Invalid role passed: {role}", ErrorType.OTHER);
                }
                String RoleToAdd = Enum.GetName(role);
                if (!await roleManager.RoleExistsAsync(RoleToAdd))
                {
                    throw new CustomException($"Invalid role passed: {role}", ErrorType.OTHER);
                }
            }
            //remover todos os roles do user primeiro, e depois adicioná-lo a todos os roles na lista passada
            IList<string> UserRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, UserRoles);
            foreach (UserRole role in roles)
            {
                String RoleToAdd = Enum.GetName(role);
                await userManager.AddToRoleAsync(user, RoleToAdd);
            }
        }
        public async Task FollowUser(User user, User ToFollow)
        {
            if (user.Following.Contains(ToFollow))
            {
                throw new CustomException($"User ({user.Id}) already follows this user ({ToFollow.Id})", ErrorType.USER_ALREADY_FOLLOWING);
            }
            if (user == ToFollow)
            {
                throw new CustomException("An user can't follow itself.", ErrorType.USER_NOT_FOLLOW_SELF);
            }
            user.Following.Add(ToFollow);
            await _context.SaveChangesAsync();
        }
        public async Task UnfollowUser(User user, User ToUnfollow)
        {
            if (!user.Following.Contains(ToUnfollow))
            {
                throw new CustomException($"User ({user.Id}) doesn't follow this user ({ToUnfollow.Id})", ErrorType.USER_NOT_FOLLOWING);
            }
            user.Following.Remove(ToUnfollow);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImage(User user)
        {
            if (user.Photo != null)
            {
                await _storageHelper.Delete(user.Photo.StorageName);
                _context.Attachments.Remove(user.Photo);
                user.Photo = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
