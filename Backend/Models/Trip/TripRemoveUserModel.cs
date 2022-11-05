using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Trip
{
    public class TripRemoveUserModel
    {
        [Required]
        public int TripId { get; set; }
        [Required]
        public string UserId { get; set; }
        //eventualmente adicionar o motivo para fins de logging
    }
}
