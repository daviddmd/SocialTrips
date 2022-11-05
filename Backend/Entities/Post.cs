using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public String Description { get; set; }
        public virtual User User { get; set; }
        public virtual Trip Trip { get; set; }
        public DateTime Date { get; set; } //a data em que as imagens foram tiradas/ocorreram
        public DateTime PublishedDate { get; set; } //a data em que o post foi atualizado
        public virtual List<Attachment> Attachments { get; set; }
        public bool IsHidden { get; set; }
    }
}
