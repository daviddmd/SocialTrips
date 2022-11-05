using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Post
{
    public class PostCreateModel
    {
        public int TripId { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
