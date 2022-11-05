using BackendAPI.Models.User;
using System;

namespace BackendAPI.Models.Trip
{
    public class TripUserModel
    {
        public UserModelSimple User { get; set; }
        public DateTime EntranceDate { get; set; }
    }
}
