using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Trip
{
    public class TripCreateModel
    {
        [Required]
        public String Name { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        public int GroupId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime BeginningDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndingDate { get; set; }
        [Required]
        public bool IsPrivate { get; set; }
    }
}
