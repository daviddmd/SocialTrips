using BackendAPI.Entities.Enums;
using System;

namespace BackendAPI.Entities
{
    public class TripInvite
    {
        public Guid Id { get; set; }
        public virtual User User { get; set; }
        public virtual Trip Trip { get; set; }
        public DateTime InvitationDate { get; set; }
    }
}
