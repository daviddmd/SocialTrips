using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Models.Group;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Group Repository for the Group Instances
    /// </summary>
    public interface IGroupRepository
    {
        /// <summary>
        /// Get all the groups
        /// </summary>
        /// <returns>List of <see cref="Group"/></returns>
        Task<IEnumerable<Group>> GetAll();
        /// <summary>
        /// Get a Group by its ID
        /// </summary>
        /// <param name="Id">ID of the Group</param>
        /// <returns>Group Entity</returns>
        Task<Group> GetById(int Id);
        /// <summary>
        /// Create a Group
        /// </summary>
        /// <param name="group">Group Entity</param>
        /// <returns></returns>
        Task Create(Group group);
        /// <summary>
        /// Update a <see cref="Group"/> details with its <see cref="GroupDetailsUpdateModel"/>
        /// </summary>
        /// <param name="group">Group whose details will be updated</param>
        /// <param name="model">Group Update Model containing the group's details to possibly be updated</param>
        /// <param name="IsAdmin">Flag to check whether the current user is a system administrator, as the systems administrator may feature the group</param>
        /// <returns></returns>
        Task Update(Group group, GroupDetailsUpdateModel model, bool IsAdmin);
        /// <summary>
        /// Get all members of a group
        /// </summary>
        /// <param name="group">Group to get all members from</param>
        /// <returns></returns>
        Task<IEnumerable<UserGroup>> GetMembers(Group group);
        /// <summary>
        /// Kicks all users and invites from a group and hides it, making it so only system administrators may see/join the group.
        /// </summary>
        /// <param name="group">Group to "Delete"</param>
        /// <returns></returns>
        Task Delete(Group group);
        /// <summary>
        /// Adds an User to a Group, with or without an invite (mandatory if private). Also checks for the user's ban status in the group. 
        /// Fails if the user is already in the group.
        /// </summary>
        /// <param name="group">Group to add an user to</param>
        /// <param name="user">User to be added to a group</param>
        /// <param name="InviteId">Invite ID (mandatory if the group is private and the user isn't a manager of the system)</param>
        /// <param name="IsManager">Whether or not the current user is a manager of the system</param>
        /// <returns></returns>
        Task AddUser(Group group, User user, Guid? InviteId, bool IsManager);
        /// <summary>
        /// Removes an user from a Group. Removes the user from all the trips of the group and removes all invites created by the user.
        /// Fails if the user is the last manager in the group or if the user isn't in the group.
        /// </summary>
        /// <param name="group">Group to remove an user from</param>
        /// <param name="user">User to be removed from a group</param>
        /// <returns></returns>
        Task RemoveUser(Group group, User user);
        /// <summary>
        /// Adds an invite to an user for a group. Fails if an invite was already sent or the user is already in the group.
        /// </summary>
        /// <param name="groupInvite">Group Invite with the Invitation Date, User that was invited, Group that the user was invited to and the unique GUID of the invite</param>
        /// <returns></returns>
        Task InviteUser(GroupInvite groupInvite);
        /// <summary>
        /// Removes a Group Invite.
        /// </summary>
        /// <param name="invite">Group Invite to rescind</param>
        /// <returns></returns>
        Task RemoveInvite(GroupInvite invite);
        /// <summary>
        /// Updates the <see cref="UserRole"/> of an User in the Group.
        /// </summary>
        /// <param name="group">Group</param>
        /// <param name="user">User</param>
        /// <param name="role">New role of the user in the group</param>
        /// <returns></returns>
        Task UpdateUserRole(Group group, User user, UserGroupRole role);
        /// <summary>
        /// Gets the Group Invite Entity by its GUID.
        /// </summary>
        /// <param name="Id">GUID of the Group Invite</param>
        /// <returns></returns>
        Task<GroupInvite> GetGroupInviteById(Guid? Id);
        /// <summary>
        /// Updates the Image of the Group.
        /// </summary>
        /// <param name="group">Group to Update the Image</param>
        /// <param name="file">Form File containing the new Image of the Group</param>
        /// <returns></returns>
        Task UpdateImage(Group group, IFormFile file);
        /// <summary>
        /// Removes the Image from the Group
        /// </summary>
        /// <param name="group">Group to Remove the Image from</param>
        /// <returns></returns>
        Task RemoveImage(Group group);
        /// <summary>
        /// Get the role from an user in a group. Fails if the User isn't in the group.
        /// </summary>
        /// <param name="group">Group</param>
        /// <param name="user">User</param>
        /// <returns>Role of the User in the Group</returns>
        Task<UserGroupRole> GetUserRole(Group group, User user);
        /// <summary>
        /// Gets the User Group Entity, containing the user's role and entrance date in the group. Fails if the user isn't in the Group.
        /// </summary>
        /// <param name="group">Group</param>
        /// <param name="user">User</param>
        /// <returns>User Group Instance</returns>
        Task<UserGroup> GetUserGroup(Group group, User user);
        /// <summary>
        /// Bans an User From a Group. Fails if the user was already banned, the ban date is in the past, the user isn't in the group or the user is the last manager in the group.
        /// </summary>
        /// <param name="group">Group to Ban an User From</param>
        /// <param name="user">User to be Banned</param>
        /// <param name="BanReason">Reason of the Ban</param>
        /// <param name="BanUntil">Length of the Ban (unspecified for it being permanent)</param>
        /// <param name="HidePosts">Hide all posts from the user in that group</param>
        /// <returns></returns>
        Task BanUser(Group group, User user, string BanReason, DateTime? BanUntil, bool HidePosts);
        /// <summary>
        /// Unbans an user from a group. Fails if the user wasn't banned from the group.
        /// </summary>
        /// <param name="group">Group to Unban an user from</param>
        /// <param name="user">User to unban from a group</param>
        /// <returns></returns>
        Task UnbanUser(Group group, User user);
        /// <summary>
        /// Gets the Group Ban entity by its identifier.
        /// </summary>
        /// <param name="Id">Identifier of the ban</param>
        /// <returns>Entity of the Group Ban</returns>
        Task<GroupBan> GetBanById(int Id);
    }
}
