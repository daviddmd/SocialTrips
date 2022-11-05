using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class UserLoginModel
    {
        [Required]
        public string EmailOrUserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public bool RememberMe { get; set; }
    }
}
