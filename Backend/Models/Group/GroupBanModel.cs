using BackendAPI.Models.User;
using System;

namespace BackendAPI.Models.Group
{
    public class GroupBanModel
    {
        public int Id { get; set; }
        public DateTime BanDate { get; set; }
        public DateTime BanUntil { get; set; }
        public string BanReason { get; set; }
        public UserModelSimple User { get; set; }
    }
}
