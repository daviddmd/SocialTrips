using BackendAPI.Entities.Enums;
using System.Collections.Generic;

namespace BackendAPI.Models.User
{
    public class UpdateUserRoleModel
    {
        public List<UserRole> Roles { get; set; }
    }
}
