using BackendAPI.Models.User;
using System;

namespace BackendAPI.Models.Group
{
    public class GroupInviteModel
    {
        public Guid Id { get; set; }
        //isto é para os gestores/admins do grupo. O JSON não precisa de ter mais informação acerca do user do que o estritamente necessário
        public UserModelSimple User { get; set; }
        //public GroupModelSimple Group { get; set; }
        public DateTime InvitationDate { get; set; }
    }
}
