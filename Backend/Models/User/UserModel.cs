using BackendAPI.Models.Post;
using BackendAPI.Models.Ranking;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.User
{
    //para utilizadores da comunidade geral
    public class UserModel
    {
        //não tem segredos, emails e IDs de third party
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public double TravelledKilometers { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string PhoneNumber { get; set; }
        public RankingModel Ranking { get; set; }
        public List<UserGroupModel> Groups { get; set; } //lista de grupos em que o utilizador se situa
        public List<PostModelUser> Posts { get; set; } //lista de últimos posts do utilizador no seu perfil
        public List<UserTripModel> Trips { get; set; } //lista de viagens em que o utilizador se situa
        public DateTime CreationDate { get; set; }
        public List<UserModelSimple> Followers { get; set; }
        public List<UserModelSimple> Following { get; set; }
    }
}
