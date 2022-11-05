using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Group
{
    public class GroupLeaveModel
    {
        [Required]
        public int GroupId { get; set; }
    }
}
