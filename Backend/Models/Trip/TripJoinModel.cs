using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Trip
{
    public class TripJoinModel
    {
        [Required]
        public int TripId { get; set; }
        public Guid? InviteId { get; set; }
    }
}
