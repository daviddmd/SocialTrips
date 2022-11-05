using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Authentication
{
    public class ResendEmailConfirmationModel
    {
        [Required]
        public string EmailOrUsername { get; set; }
    }
}
