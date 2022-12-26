using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class ResetPasswordModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string PasswordResetToken { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
