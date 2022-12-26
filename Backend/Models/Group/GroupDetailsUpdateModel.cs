namespace BackendAPI.Models.Group
{
    public class GroupDetailsUpdateModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsFeatured { get; set; }
    }
}
