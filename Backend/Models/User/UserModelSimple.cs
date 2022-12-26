using BackendAPI.Models.Ranking;
using System;

namespace BackendAPI.Models.User
{
    public class UserModelSimple
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public double TravelledKilometers { get; set; }
        /*
         * Faz mais sentido estes atributos estarem no perfil do utilizador completo, e não parcial
         * Pode-se pensar neste modelo como a "preview" do utilizador na rede social
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string PhoneNumber { get; set; }
        */
        public RankingModel Ranking { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
