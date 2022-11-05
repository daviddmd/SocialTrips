using BackendAPI.Models.Trip;
using System;

namespace BackendAPI.Models.User
{
    public class UserTripInviteModel
    {
        public Guid Id { get; set; }
        public TripModelSimple Trip { get; set; }
        public DateTime InvitationDate { get; set; }
    }
}
