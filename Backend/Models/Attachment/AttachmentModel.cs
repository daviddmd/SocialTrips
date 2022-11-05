using System;

namespace BackendAPI.Models.Attachment
{
    public class AttachmentModel
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; }
        public string Url { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
