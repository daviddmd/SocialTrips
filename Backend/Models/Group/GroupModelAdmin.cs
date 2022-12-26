using BackendAPI.Models.Trip;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Group
{
    public class GroupModelAdmin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public String Image { get; set; }
        public List<GroupUserModel> Users { get; set; }
        public DateTime CreationDate { get; set; }
        public bool HasExperiencedUser { get; set; }
        public Boolean IsPrivate { get; set; }
        public List<GroupInviteModel> Invites { get; set; }
        //futuramente terá um activity log
        public List<TripModelGroup> Trips { get; set; }
        public double AverageTripCost { get; set; }
        public double AverageTripDistance { get; set; }
        public Boolean IsFeatured { get; set; }
        public List<GroupEventModel> Events { get; set; }
        public virtual List<GroupBanModel> Bans { get; set; }
    }
}
