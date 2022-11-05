using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Activity
{
    public class ActivityRemoveModel
    {
        [Required]
        public int TripId { get; set; }
        [Required]
        public int ActivityId { get; set; }
    }
}
