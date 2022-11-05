using System;

namespace BackendAPI.Entities
{
    public class GroupBan
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
        public string BanReason { get; set; }
        public DateTime BanDate { get; set; }
        //null se indefinido/permanente
        public DateTime? BanUntil { get; set; }
    }
}
