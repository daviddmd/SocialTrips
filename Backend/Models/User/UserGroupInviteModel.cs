using BackendAPI.Entities.Enums;
using BackendAPI.Models.Group;
using System;

namespace BackendAPI.Models.User
{
    public class UserGroupInviteModel
    {
        public Guid Id { get; set; }
        //isto é para o utilizador em questão. O JSON não precisa de ter mais informação acerca do grupo do que o estritamente necessário, visto que o utilizador ainda não se juntou ao mesmo
        public GroupModelSimple Group { get; set; }
        //public GroupModelSimple Group { get; set; }
        public DateTime InvitationDate { get; set; }
    }
}
