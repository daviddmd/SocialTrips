using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Group
{
    public class GroupRemoveUserModel
    {
        [Required]
        public int GroupId { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
