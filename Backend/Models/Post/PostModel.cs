using BackendAPI.Models.Attachment;
using BackendAPI.Models.Trip;
using BackendAPI.Models.User;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Post
{
    /*
     * Este modelo será para ver um post com um hyperlink com ID, i.e. /api/Posts/1
     * Para isto, necessita de ter a referência da Trip
     * Apenas é possível remover/esconder posts se for administrador ou o próprio utilizador do sistema a partir desta interface
     */
    public class PostModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public UserModelSimple User { get; set; }
        public TripModelSimple Trip{ get; set; }
        public DateTime Date { get; set; }
        public DateTime PublishedDate { get; set; }
        public List<AttachmentModel> Attachments { get; set; }
        public bool IsHidden { get; set; }
    }
}
