using BackendAPI.Models.Trip;
using System;

namespace BackendAPI.Models.User
{
    public class UserTripModel
    {
        public TripModelSimple Trip { get; set; }
        public DateTime EntranceDate { get; set; }
    }
}
