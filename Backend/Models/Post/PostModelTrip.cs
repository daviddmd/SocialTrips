using BackendAPI.Models.Attachment;
using BackendAPI.Models.Trip;
using BackendAPI.Models.User;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Post
{
    /*
     * Este modelo será para ver um post numa lista de Posts da viagem
     * Para tal, não necessita da referência da viagem
     * Apenas é possível remover/esconder posts directamente da interface da viagem se for gestor do grupo, administrador do sistema ou o próprio utilizador
     */
    public class PostModelTrip
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public UserModelSimple User { get; set; }
        public DateTime Date { get; set; }
        public DateTime PublishedDate { get; set; }
        public List<AttachmentModel> Attachments { get; set; }
        public bool IsHidden { get; set; }
    }
}
