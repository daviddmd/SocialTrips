using System;
using System.Collections.Generic;

namespace BackendAPI.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public virtual Attachment Image { get; set; }
        public DateTime CreationDate { get; set; }
        public Boolean IsPrivate { get; set; }
        public virtual List<UserGroup> Users { get; set; }
        public virtual List<GroupInvite> Invites { get; set; }
        public virtual List<Trip> Trips { get; set; }
        public Boolean HasExperiencedUser { get; set; }
        public double AverageTripCost { get; set; }
        public double AverageTripDistance { get; set; }
        public Boolean IsFeatured { get; set; } = false;
        public virtual List<GroupEvent> Events { get; set; }
        public virtual List<GroupBan> Bans { get; set; }
    }
}
