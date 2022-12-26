using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class UserDetailsUpdateModel
    {
        //não vamos permitir atualizar outros detalhes por enquanto
        [Required]
        public string Name { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string City { get; set; }
        public string Description { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string PhoneNumber { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string Locale { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
