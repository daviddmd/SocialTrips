using System;

namespace BackendAPI.Models.Ranking
{
    public class RankingUpdateModel
    {
        public String Description { get; set; }
        public String Name { get; set; }
        public String Color { get; set; }
        public double MinimumKilometers { get; set; }
    }
}
