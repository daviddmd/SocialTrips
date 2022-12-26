using System;

namespace BackendAPI.Models.Trip
{
    public class TripDetailsUpdateModel
    {
        public String Name { get; set; }
        //public AttachmentModel Image { get; set; } //mais tarde para atualizar a imagem um Guid da imagem destino
        public String Description { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPrivate { get; set; }
    }
}
