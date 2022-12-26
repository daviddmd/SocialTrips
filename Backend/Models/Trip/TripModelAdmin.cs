using BackendAPI.Models.Activity;
using BackendAPI.Models.Group;
using BackendAPI.Models.Post;
using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Trip
{
    public class TripModelAdmin
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Image { get; set; }
        public String Description { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public double ExpectedBudget { get; set; } //À medida que mais actividades vão sendo adicionadas, o orçamento esperado vai aumentando
        public double TotalDistance { get; set; }
        public List<ActivityModelSimple> Activities { get; set; }
        public List<PostModelTrip> Posts { get; set; }
        /*
         * Este modelo tem a lista de utilizadores do grupo de modo a permitir convidar os mesmos para a viagem (se for gestor, este modelo sendo para os gestores)
         * Tendo também a lista de utilizadores, contém também os utilizadores com os seus roles, de modo ao frontend saber que opções mostrar dependendo do role (normal, moderador ou gestor)
         */
        public GroupModelTrip Group { get; set; }
        public List<TripUserModel> Users { get; set; }
        public List<TripInviteModel> Invites { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPrivate { get; set; }
        public List<TripEventModel> Events { get; set; }
    }
}
