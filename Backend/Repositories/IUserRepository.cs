using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Repository for the System's Users (ASP.NET Core Identity)
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Get all Users
        /// </summary>
        /// <returns>All Users in the System</returns>
        Task<IEnumerable<User>> GetAll();
        /// <summary>
        /// Get user by their ASP.NET Core Identity ID
        /// </summary>
        /// <param name="Id">ID of the User</param>
        /// <returns>Entity associated with the User</returns>
        Task<User> GetById(String Id);
        /// <summary>
        /// Update User Detail with <see cref="UserDetailsUpdateModel"/>
        /// </summary>
        /// <param name="user">User to be updated</param>
        /// <param name="model">Model with New Details of User</param>
        /// <returns></returns>
        Task<IdentityResult> Update(User user, UserDetailsUpdateModel model);
        /// <summary>
        /// Search a User by their Name or Email
        /// </summary>
        /// <param name="userSearch">Name or Email of the User to Query</param>
        /// <returns>List of Users that match the Name or Email</returns>
        Task<IEnumerable<User>> Search(UserSearchModel userSearch);
        /// <summary>
        /// <list type="number">
        /// <item>Clear all the user's Trip and Group Invites</item>
        /// <item>Clear the user's following and followers list</item>
        /// <item>Remove the user from all the groups and trips it's in</item>
        /// <item>Deletes all the user's posts and associated attachments and files</item>
        /// <item>Deletes the user's profile picture</item>
        /// </list>
        /// </summary>
        /// <param name="user">User to Delete</param>
        /// <returns></returns>
        Task Delete(User user);
        /// <summary>
        /// Update the User's Profile Picture
        /// </summary>
        /// <param name="user">User to update the profile picture</param>
        /// <param name="file">Form File containing the new Profile Picture of the User</param>
        /// <returns></returns>
        Task UpdateImage(User user, IFormFile file);
        /// <summary>
        /// Update the User's Roles. An user has zero or more roles, and the roles are set all at once, with a list. 
        /// An user without a role has no permissions in the system. The roles must be valid, as defined by <see cref="UserRole"/>.
        /// </summary>
        /// <param name="user">User to update roles</param>
        /// <param name="roles">List of Roles to associate with the user</param>
        /// <returns></returns>
        Task UpdateUserRoles(User user, List<UserRole> roles);
        /// <summary>
        /// Follow an user in the system. Adds the user to be followed to the followed list of the user that follows such user, and adds the following user to the followers list of the followed user.
        /// </summary>
        /// <param name="user">User that follows</param>
        /// <param name="ToFollow">User that is to be followed</param>
        /// <returns></returns>
        Task FollowUser(User user, User ToFollow);
        /// <summary>
        /// Unfollows an user in the system.  Removes the user to be unfollowed from the followed list of the user that unfollows such user,
        /// and removes the following user from the followers list of the, now, unfollowed user.
        /// </summary>
        /// <param name="user">User that unfollows</param>
        /// <param name="ToUnfollow">User that is to be unfollowed</param>
        /// <returns></returns>
        Task UnfollowUser(User user, User ToUnfollow);
        /// <summary>
        /// Delete the User's Profile Picture
        /// </summary>
        /// <param name="user">User to Remove the Profile Picture from</param>
        /// <returns></returns>
        Task DeleteImage(User user);
    }
}
