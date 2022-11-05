using BackendAPI.Entities.Enums;
using System;

namespace BackendAPI.Entities
{
    public class TripEvent
    {
        public int Id { get; set; }
        public virtual Trip Trip { get; set; }
        public EventType EventType { get; set; }
        public DateTime Date { get; set; }
        public virtual User User { get; set; }
    }
}
