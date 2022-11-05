using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Activity
{
    public class ActivitySearchTransportModel
    {
        [Required]
        public DateTime DepartTime { get; set; }
        [Required]
        public string OriginPlaceId { get; set; }
        [Required]
        public string DestinationPlaceId { get; set; }
        public string CountryCode { get; set; }
    }
}