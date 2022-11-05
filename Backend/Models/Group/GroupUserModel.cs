using BackendAPI.Entities.Enums;
using BackendAPI.Models.User;
using System;

namespace BackendAPI.Models.Group
{
    public class GroupUserModel
    {
        public UserModelSimple User { get; set; } //modelo com detalhes estritamente necessários do grupo
        public DateTime EntranceDate { get; set; }
        public UserGroupRole Role { get; set; }
    }
}
