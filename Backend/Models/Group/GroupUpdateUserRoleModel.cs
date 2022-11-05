using BackendAPI.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Group
{
    public class GroupUpdateUserRoleModel
    {
        [Required]
        public int GroupId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public UserGroupRole UserGroupRole { get; set; }
    }
}
