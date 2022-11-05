using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Trip
{
    public class TripLeaveModel
    {
        [Required]
        public int TripId { get; set; }
    }
}
