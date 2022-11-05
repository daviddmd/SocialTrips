using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class ForgotPasswordModel
    {
        [Required]
        public string EmailOrUsername { get; set; }
    }
}
