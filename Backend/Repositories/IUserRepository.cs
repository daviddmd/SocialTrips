using BackendAPI.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using BackendAPI.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using BackendAPI.Entities.Enums;

namespace BackendAPI.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();
        Task<User> GetById(String Id);
        Task<IdentityResult> Update(User user, UserDetailsUpdateModel model);
        Task<IEnumerable<User>> Search(UserSearchModel userSearch);
        Task Delete(User user);
        Task UpdateImage(User user, IFormFile file);
        Task UpdateUserRoles(User user, List<UserRole> roles);
        Task FollowUser(User user, User ToFollow);
        Task UnfollowUser(User user, User ToUnfollow);
        Task DeleteImage(User user);
    }
}
