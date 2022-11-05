using System;
using BackendAPI.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Entities
{
    public class UserGroup
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
        public DateTime EntranceDate { get; set; }
        public UserGroupRole Role { get; set; }
    }
}