using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Models.Group;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAll();
        Task<Group> GetById(int Id);
        //adicionar mais tarde password reset token se user type for por email
        Task Create(Group group);
        Task Update(Group group, GroupDetailsUpdateModel model, bool IsAdmin);
        Task<IEnumerable<UserGroup>> GetMembers(Group group);
        Task Delete(Group group);
        Task AddUser(Group group, User user, Guid? InviteId, bool IsManager);
        Task RemoveUser(Group group,User user);
        Task InviteUser(GroupInvite groupInvite);
        Task RemoveInvite(GroupInvite invite);
        Task UpdateUserRole(Group group, User user, UserGroupRole role);
        Task<GroupInvite> GetGroupInviteById(Guid? Id);
        Task UpdateImage(Group group, IFormFile file);
        Task RemoveImage(Group group);
        Task<UserGroupRole> GetUserRole(Group group, User user);
        Task<UserGroup> GetUserGroup(Group group, User user);
        Task BanUser(Group group, User user, string BanReason, DateTime? BanUntil, bool HidePosts);
        Task UnbanUser(Group group, User user);
        Task<GroupBan> GetBanById(int Id);
    }
}
