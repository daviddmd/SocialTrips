using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Trip
{
    public class TripSendInviteModel
    {
        [Required]
        public int TripId { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
