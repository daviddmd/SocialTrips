using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class UserRegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(2, ErrorMessage = "ISO 2-Character Country Code.")]
        public string Country { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Locale { get; set; }
        //não são obrigatórios
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
    }
}
