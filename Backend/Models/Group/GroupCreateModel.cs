using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Group
{
    public class GroupCreateModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool IsPrivate { get; set; }
    }
}
