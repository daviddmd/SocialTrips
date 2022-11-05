using BackendAPI.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Entities
{
    public class GroupInvite
    {
        public Guid Id { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
        public DateTime InvitationDate { get; set; }
    }
}
