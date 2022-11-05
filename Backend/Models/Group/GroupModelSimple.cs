using BackendAPI.Models.Attachment;
using System;

namespace BackendAPI.Models.Group
{
    public class GroupModelSimple
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public String Image { get; set; }
        public DateTime CreationDate { get; set; }
        public bool HasExperiencedUser { get; set; }
        public Boolean IsPrivate { get; set; }
        public double AverageTripCost { get; set; }
        public double AverageTripDistance { get; set; }
        public Boolean IsFeatured { get; set; }
    }
}
