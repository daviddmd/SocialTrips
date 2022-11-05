using System;

namespace BackendAPI.Entities
{
    public class UserTrip
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public virtual Trip Trip { get; set; } //vai relacionar com o Trip.Group.Users e daí verificar se é gestor
        public DateTime EntranceDate { get; set; }
    }
}
