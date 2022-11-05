using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Post
{
    public class PostUpdateModel
    {
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsHidden { get; set; }
    }
}
