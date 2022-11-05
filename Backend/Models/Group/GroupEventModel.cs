using BackendAPI.Entities.Enums;
using BackendAPI.Models.User;
using System;

namespace BackendAPI.Models.Group
{
    public class GroupEventModel
    {
        public EventType EventType { get; set; }
        public DateTime Date { get; set; }
        public UserModelSimple User { get; set; }
    }
}
