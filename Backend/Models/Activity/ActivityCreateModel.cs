using BackendAPI.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Activity
{
    public class ActivityCreateModelInvidual
    {
        //tem as regras em relação ao transporte
        [Required]
        public DateTime BeginningDate { get; set; }
        [Required]
        public DateTime EndingDate { get; set; }
        //definido automaticamente pelo frontend, pode ser modificado. se for vazio, obter a morada a partir do place ID
        public string Address { get; set; }
        //no caso do transporte é definido automaticamente pela linha, pode ser modificado
        public string Description { get; set; }
        public string GooglePlaceId { get; set; }
        [Required]
        public double ExpectedBudget { get; set; }
        [Required]
        public ActivityType ActivityType { get; set; }
        [Required]
        public TransportType TransportType { get; set; }
    }
    public class ActivityCreateModel
    {
        //the trip ID for the trip itself
        public int TripId { get; set; }
        //the activity, with the transport type nulled
        [Required]
        public ActivityCreateModelInvidual ActivityCreate { get; set; }
        //the transport activity, to be placed between the previous one, and the one to add. can be empty if it's the first one
        public ActivityCreateModelInvidual ActivityTransport { get; set; }
    }
}
