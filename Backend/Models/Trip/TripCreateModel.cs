using BackendAPI.Entities;
using BackendAPI.Models.Attachment;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Models.Trip
{
    public class TripCreateModel
    {
        [Required]
        public String Name { get; set; }
        [Required]
        public String Description { get; set; }
        [Required]
        public int GroupId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime BeginningDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndingDate { get; set; }
        [Required]
        public bool IsPrivate { get; set; }
    }
}
