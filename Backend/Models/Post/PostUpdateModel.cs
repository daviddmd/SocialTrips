using System;

namespace BackendAPI.Models.Post
{
    public class PostUpdateModel
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsHidden { get; set; }
    }
}
