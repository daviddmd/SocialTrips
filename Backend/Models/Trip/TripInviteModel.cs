using BackendAPI.Models.User;
using System;

namespace BackendAPI.Models.Trip
{
    public class TripInviteModel
    {
        public Guid Id { get; set; }
        public UserModelSimple User { get; set; }
        public DateTime InvitationDate { get; set; }
    }
}
