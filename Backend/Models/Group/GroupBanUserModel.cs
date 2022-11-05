using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Group
{
    public class GroupBanUserModel
    {
        [Required]
        public int GroupId { get; set; }
        [Required]
        public string UserId { get; set; }
        public DateTime? BanUntil { get; set; }
        [Required]
        public string BanReason { get; set; }
        [Required]
        public bool HidePosts { get; set; } = false;
    }
}
