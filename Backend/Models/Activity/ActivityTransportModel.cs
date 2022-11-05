using BackendAPI.Entities.Enums;
using System;

namespace BackendAPI.Models.Activity
{
    public class ActivityTransportModel
    {

        public TransportType TransportType { get; set; }
        public string Description { get; set; }
        public int Distance { get; set; } //em metros
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
