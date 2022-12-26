using System;
using System.Collections.Generic;

namespace BackendAPI.Models.Trip
{
    public class TripModelGroup
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Image { get; set; }
        public String Description { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public double ExpectedBudget { get; set; } //À medida que mais actividades vão sendo adicionadas, o orçamento esperado vai aumentando
        public double TotalDistance { get; set; }
        /*
         * Não precisa de ter as actividades e as publicações no modelo da viagem no grupo
         * public List<ActivityModelSimple> Activities { get; set; } //eventualmente condensar estes dois para formatos mais simples para evitar recursividade
         * public List<PostModelTrip> Posts { get; set; }
         */
        public List<TripUserModel> Users { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPrivate { get; set; }
    }
}
