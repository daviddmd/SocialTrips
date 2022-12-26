using BackendAPI.Entities.Enums;
using System.Collections.Generic;

namespace BackendAPI.Models.Information
{
    public class StatisticsModel
    {
        public Dictionary<string, int> MostVisitedPlaces { get; set; }
        public Dictionary<string, double> TripsByTotalDistance { get; set; }
        public Dictionary<string, double> TripsByTotalCost { get; set; }
        public Dictionary<string, int> TripsByNumberUsers { get; set; }
        public Dictionary<string, double> GroupsByAverageDistance { get; set; }
        public Dictionary<string, double> GroupsByAverageCost { get; set; }
        public Dictionary<string, int> GroupsByNumberUsers { get; set; }
        public Dictionary<string, int> RankingUserDistribution { get; set; }
        public Dictionary<TransportType, int> TransportTypeDistribution { get; set; }
        public Dictionary<ActivityType, int> ActivityTypeDistribution { get; set; }
    }
}
