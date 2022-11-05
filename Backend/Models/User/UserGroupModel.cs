using BackendAPI.Entities.Enums;
using BackendAPI.Models.Group;
using System;

namespace BackendAPI.Models.User
{
    public class UserGroupModel
    {
        public GroupModelSimple Group { get; set; } //novo model para apenas incluir os detalhes estritamente necessários do grupo
        public DateTime EntranceDate { get; set; }
        public UserGroupRole Role { get; set; }
    }
}
