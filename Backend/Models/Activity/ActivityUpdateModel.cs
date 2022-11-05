using BackendAPI.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Activity
{
    public class ActivityUpdateModel
    {
        [Required]
        public DateTime BeginningDate { get; set; }
        [Required]
        public DateTime EndingDate { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Description { get; set; }
        public double ExpectedBudget { get; set; } = 0;
        [Required]
        public ActivityType ActivityType { get; set; }
        [Required]
        public TransportType TransportType { get; set; }
    }
}
