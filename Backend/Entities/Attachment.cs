using System;

namespace BackendAPI.Entities
{
    public class Attachment
    {
        //O ID será usado para realizar a remoção por ajax
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; }
        public string StorageName { get; set; }
        public string Url { get; set; }
        public DateTime UploadedDate { get; set; }
        public virtual Post Post { get; set; }
    }
}
