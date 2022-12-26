using BackendAPI.Data;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Helpers;
using BackendAPI.Models.Group;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DatabaseContext _context;
        private readonly IGoogleCloudStorageHelper _storageHelper;

        public GroupRepository(DatabaseContext context, IGoogleCloudStorageHelper storageHelper)
        {
            _context = context;
            _storageHelper = storageHelper;
        }


        //na entrada de um membro recalcular se o mesmo tem experiência, e se sim, atualizar o atributo. na saida de um membro, verificar se o grupo continua a ter experiência, e se sim, manter, caso contrário...
        /// <summary>
        /// Checks whether or not a Group has an experienced member. Every time a user joins or leaves a group, a group may gain or lose an experienced member, so this is checked on both of these events
        /// and this information may be associated with the group's details.
        /// </summary>
        /// <param name="group">Group to check if any experienced member is present</param>
        /// <returns>The existence of any experienced member in a group</returns>
        public async Task<bool> GroupHasExperiencedMember(Group group)
        {
            return await _context.UserGroups.AnyAsync(userGroup => userGroup.User.TravelledKilometers > 1000 && userGroup.Group == group);
        }
        public async Task AddUser(Group group, User user, Guid? InviteId, bool IsManager)
        {
            GroupBan ban = await _context.GroupBans.Where(gb=>gb.User == user && gb.Group == group).FirstOrDefaultAsync();
            if (ban != null)
            {
                //unban user and continue join process
                if (ban.BanUntil != null && ban.BanUntil < DateTime.Now)
                {
                    _context.GroupBans.Remove(ban);
                    GroupEvent groupEvent = new()
                    {
                        EventType = EventType.GROUP_USER_UNBAN,
                        Group = group,
                        User = user,
                        Date = DateTime.Now
                    };
                    _context.GroupEvents.Add(groupEvent);
                }
                else
                {
                    throw new CustomException("This user is banned.", ErrorType.GROUP_USER_JOIN_BANNED);
                }
            }
            bool WasInvited = false;
            GroupInvite invite = await GetGroupInviteById(InviteId);
            if (group.IsPrivate && !IsManager)
            {
                if (invite == null || (invite.Group != group || invite.User != user))
                {
                    throw new CustomException("Invalid or non-existing invite.", ErrorType.GROUP_INVITE_INVALID);
                }
            }
            //Remover o convite se tiver sido utilizado um para juntar ao grupo
            if (invite != null && group.IsPrivate)
            {
                WasInvited = true;
                _context.GroupInvites.Remove(invite);
                await _context.SaveChangesAsync();
            }
            UserGroup userGroup = await GetUserGroup(group, user);
            if (userGroup == null)
            {
                int NumberMembersGroup = await _context.UserGroups.CountAsync(ug=>ug.Group.Id==group.Id);
                //caso não hajam gestores no grupo, o primeiro a entrar será gestor. Administrador apesar de ter role de regular será sempre administrador e pode chamar os endpoints com permissões.
                UserGroup userGroupCreate =  new(){ User = user, Group=group, EntranceDate = DateTime.Now, Role= NumberMembersGroup==0?UserGroupRole.MANAGER:UserGroupRole.REGULAR};
                _context.UserGroups.Add(userGroupCreate);
                await _context.SaveChangesAsync();
                group.HasExperiencedUser = await GroupHasExperiencedMember(group);
                _context.Groups.Update(group);
                GroupEvent groupEvent = new()
                {
                    EventType = WasInvited ? EventType.GROUP_USER_ENTER_INVITE : EventType.GROUP_USER_ENTER,
                    Group = group,
                    User = user,
                    Date = DateTime.Now
                };
                _context.GroupEvents.Add(groupEvent);
                //Se o utilizador se juntou a um grupo público, mas foi previamente convidado
                GroupInvite existingInvite = await _context.GroupInvites.Where(gi => gi.User == user && gi.Group == group).FirstOrDefaultAsync();
                if (existingInvite != null)
                {
                    _context.GroupInvites.Remove(existingInvite);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("User is Already in the Group", ErrorType.GROUP_USER_ALREADY_PRESENT);
            }
        }
        public async Task Create(Group group)
        {
            group.CreationDate = DateTime.Now;
            group.HasExperiencedUser = false;
            _context.Groups.Add(group);
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_CREATE,
                Group = group,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }
        /*
         * Métodos Delete não são desejáveis, porque há bastante meta-informação associada a um grupo.
         * Ao invés, pretende-se limpar todos os utilizadores, convites e esconder o grupo. Assim apenas administradores conseguirão aceder/juntar-se ao grupo
         */
        public async Task Delete(Group group)
        {
            group.IsPrivate = true;
            group.Users.Clear();
            group.Invites.Clear();
            //Remover todos os utilizadores e convites das viagens e esconder todos os posts desta
            foreach(Trip trip in group.Trips)
            {
                trip.Users.Clear();
                trip.Invites.Clear();
                trip.IsPrivate = true;
                trip.Posts.ForEach(post => post.IsHidden = true);
                _context.Posts.UpdateRange(trip.Posts);
                _context.Trips.Update(trip);
            }
            _context.Groups.Update(group);
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_DELETE,
                Group = group,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Group>> GetAll()
        {
            return await _context.Groups.AsQueryable().ToListAsync();
        }

        public async Task<Group> GetById(int Id)
        {
            return await _context.Groups.Where(group => group.Id == Id).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<UserGroup>> GetMembers(Group group)
        {
            return await _context.UserGroups.Where(userGroup => userGroup.Group == group).ToListAsync();
        }

        public async Task<UserGroup> GetUserGroup(Group group, User user)
        {
            return await _context.UserGroups.Where(userGroup => userGroup.User == user && userGroup.Group == group).SingleOrDefaultAsync();
        }
        public async Task InviteUser(GroupInvite groupInvite)
        {
            //novos convites apenas podem ser enviados se os anteriores tiverem sido consumidos. isto irá procurar por um convite existente
            if (_context.GroupInvites.Any(gi=>gi.User.Id==groupInvite.User.Id && gi.Group.Id == groupInvite.Group.Id))
            {
                throw new CustomException("User was already invited", ErrorType.GROUP_USER_ALREADY_INVITED);
            }
            if (_context.UserGroups.Any(ug => ug.User.Id == groupInvite.User.Id && ug.Group.Id == groupInvite.Group.Id))
            {
                throw new CustomException("User is already on this group", ErrorType.GROUP_USER_ALREADY_PRESENT);
            }
            _context.GroupInvites.Add(groupInvite);
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_INVITE_CREATE,
                Group = groupInvite.Group,
                User = groupInvite.User,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<UserGroupRole> GetUserRole(Group group,User user)
        {
            UserGroup ug = await GetUserGroup(group, user);
            if (ug == null)
            {
                return UserGroupRole.NONE;
                //throw new CustomException("This user isn't in this group", ErrorType.GROUP_USER_NOT_PRESENT);
            }
            return ug.Role;
        }

        public async Task RemoveUser(Group group, User user)
        {
            UserGroup userGroup = await GetUserGroup(group, user);
            if (userGroup != null)
            {
                int NumberManagers = await _context.UserGroups.Where(ug=>ug.Role==UserGroupRole.MANAGER && ug.Group.Id == group.Id).CountAsync();
                //um gestor não se pode demover a ele próprio, apenas a outros, logo vai haver sempre pelo menos 1
                if (NumberManagers == 1 && userGroup.Role==UserGroupRole.MANAGER)
                {
                    throw new CustomException("Group only has one manager left.", ErrorType.GROUP_LAST_MANAGER_LEAVE);
                }
                _context.UserGroups.Remove(userGroup);
                //Remover o utilizador de todas as viagens do grupo
                _context.UserTrips.RemoveRange(_context.UserTrips.Where(ut => ut.User == user && ut.Trip.Group == group));
                //Remover todos os convites do utilizador para viagens no grupo e convites no grupo
                _context.GroupInvites.RemoveRange(_context.GroupInvites.Where(gi => gi.Group == group && gi.User == user));
                _context.TripInvites.RemoveRange(_context.TripInvites.Where(ut => ut.User == user && ut.Trip.Group == group));
                await _context.SaveChangesAsync();
                group.HasExperiencedUser = await GroupHasExperiencedMember(group);
                _context.Groups.Update(group);
                GroupEvent groupEvent = new()
                {
                    EventType = EventType.GROUP_USER_LEAVE,
                    Group = group,
                    User = user,
                    Date = DateTime.Now
                };
                _context.GroupEvents.Add(groupEvent);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("This user isn't in this group", ErrorType.GROUP_USER_NOT_PRESENT);
            }
        }
        public async Task Update(Group group, GroupDetailsUpdateModel model, bool IsAdmin)
        {
            group.Name = model.Name;
            group.Description = model.Description;
            group.IsPrivate = model.IsPrivate;
            //apenas um administrador pode tornar ou remover um grupo de destacado
            if (IsAdmin)
            {
                group.IsFeatured = model.IsFeatured;
            }
            _context.Groups.Update(group);
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_DETAILS_UPDATE,
                Group = group,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserRole(Group group, User user, UserGroupRole role)
        {
            if (!Enum.IsDefined(role))
            {
                throw new CustomException("Invalid Role", ErrorType.GROUP_ROLE_INVALID);
            }
            UserGroup userGroup = await _context.UserGroups.Where(ug => ug.User == user && ug.Group == group).FirstOrDefaultAsync();
            if (userGroup == null)
            {
                throw new CustomException("This user isn't in this group", ErrorType.GROUP_USER_NOT_PRESENT);
            }
            userGroup.Role = role;
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_USER_ROLE_CHANGE,
                Group = group,
                User = user,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }


        public async Task<GroupInvite> GetGroupInviteById(Guid? Id)
        {
            return await _context.GroupInvites.Where(gi => gi.Id == Id).FirstOrDefaultAsync();
        }

        public async Task RemoveInvite(GroupInvite invite)
        {
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_INVITE_REJECT,
                Group = invite.Group,
                User = invite.User,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            _context.GroupInvites.Remove(invite);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateImage(Group group, IFormFile file)
        {
            if (file != null && FileHelper.IsImage(file))
            {
                //delete the old one if exists
                await RemoveImage(group);
                String OriginalFileName = file.FileName;
                String DestinationFileName = $"group_{group.Id}{Path.GetExtension(file.FileName).ToLower()}";
                String Url = await _storageHelper.Upload(file, DestinationFileName);
                Attachment attachment = new() { OriginalFileName = OriginalFileName, StorageName = DestinationFileName, Url = Url, UploadedDate = DateTime.Now };
                _context.Attachments.Add(attachment);
                group.Image = attachment;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new CustomException("No valid image passed", ErrorType.MEDIA_ERROR);
            }
        }

        public async Task BanUser(Group group, User user, string BanReason, DateTime? BanUntil, bool HidePosts)
        {
            if (await _context.GroupBans.Where(gb => gb.Group == group && gb.User == user).AnyAsync())
            {
                throw new CustomException("This user was already banned", ErrorType.GROUP_USER_ALREADY_BANNED);
            }
            if (BanUntil.HasValue && BanUntil.Value.Date < DateTime.Now.Date)
            {
                throw new CustomException("The ban date cannot be in the past", ErrorType.GROUP_BAN_DATE_INVALID);
            }
            UserGroup userGroup = await GetUserGroup(group, user);
            if (userGroup == null)
            {
                throw new CustomException("This user isn't in this group", ErrorType.GROUP_USER_NOT_PRESENT);
            }
            int NumberManagers = await _context.UserGroups.Where(ug => ug.Role == UserGroupRole.MANAGER && ug.Group.Id == group.Id).CountAsync();
            if (NumberManagers == 1 && userGroup.Role == UserGroupRole.MANAGER)
            {
                throw new CustomException("Group only has one manager left.", ErrorType.GROUP_LAST_MANAGER_LEAVE);
            }
            group.Users.Remove(userGroup);
            group.HasExperiencedUser = await GroupHasExperiencedMember(group);
            _context.Groups.Update(group);
            _context.UserTrips.RemoveRange(user.Trips.Where(ut => ut.Trip.Group == group && ut.User == user));
            _context.GroupInvites.RemoveRange(user.GroupInvites.Where(gi => gi.Group == group && gi.User == user));
            _context.TripInvites.RemoveRange(user.TripInvites.Where(ut => ut.User == user && ut.Trip.Group == group));
            _context.GroupBans.Add(new GroupBan() { Group=group,User=user,BanReason= BanReason , BanUntil=BanUntil, BanDate = DateTime.Now});
            if (HidePosts)
            {
                foreach(Trip trip in group.Trips)
                {
                    foreach(Post post in trip.Posts)
                    {
                        if (post.User == user)
                        {
                            post.IsHidden = true;
                        }
                    }
                }
            }
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_USER_BAN,
                Group = group,
                User = user,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }

        public async Task UnbanUser(Group group, User user)
        {
            GroupBan ban = await _context.GroupBans.Where(gb => gb.Group == group && gb.User == user).FirstOrDefaultAsync();
            if (ban==null)
            {
                throw new CustomException("This user isn't banned", ErrorType.GROUP_USER_NOT_BANNED);
            }
            _context.GroupBans.Remove(ban);
            GroupEvent groupEvent = new()
            {
                EventType = EventType.GROUP_USER_UNBAN,
                Group = group,
                User = user,
                Date = DateTime.Now
            };
            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<GroupBan> GetBanById(int Id)
        {
            return await _context.GroupBans.Where(gb=>gb.Id == Id).FirstOrDefaultAsync();
        }

        public async Task RemoveImage(Group group)
        {
            if (group.Image != null)
            {
                await _storageHelper.Delete(group.Image.StorageName);
                _context.Attachments.Remove(group.Image);
                group.Image = null;
                await _context.SaveChangesAsync();
            }
        }
    }
}
