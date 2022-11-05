using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class ConfirmEmailModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string EmailConfirmationToken { get; set; }
    }
}
