using BackendAPI.Models.Trip;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Group
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public String Image { get; set; }
        public List<GroupUserModel> Users { get; set; }
        public DateTime CreationDate { get; set; }
        public bool HasExperiencedUser { get; set; }
        public Boolean IsPrivate { get; set; }
        public List<TripModelGroup> Trips { get; set; }
        public double AverageTripCost { get; set; }
        public double AverageTripDistance { get; set; }
        public Boolean IsFeatured { get; set; }
    }
}
