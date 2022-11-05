using BackendAPI.Models.Attachment;
using BackendAPI.Models.Trip;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Post
{
    /*
     * Este modelo será para ver um post numa lista de Posts do User
     * Para isto, não necessita de ter a referência do utilizador
     * Apenas o utilizador e administrador consegue remover os seus próprios posts a partir desta interface
     */
    public class PostModelUser
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public TripModelSimple Trip { get; set; }
        public DateTime Date { get; set; }
        public DateTime PublishedDate { get; set; }
        public List<AttachmentModel> Attachments { get; set; }
    }
}
