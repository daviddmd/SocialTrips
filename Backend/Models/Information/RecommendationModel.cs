using BackendAPI.Models.Group;
using BackendAPI.Models.Post;
using BackendAPI.Models.Trip;
using System.Collections.Generic;

namespace BackendAPI.Models.Information
{
    public class RecommendationModel
    {
        //no caso de estar logged in, exclui grupos em que o utilizador já esteja
        public List<GroupModelSimple> featuredGroups { get; set; }
        public List<GroupModelSimple> recommendedGroups { get; set; }
        public List<TripModelSimple> recommendedTrips { get; set; }
        //ultimos posts da comunidade, incluindo de viagens privadas, caso o user esteja nelas
        public List<PostModel> latestPostsCommunity { get; set; }
        //vazio no caso de ser anonimo
        public List<PostModel> latestPostsFriends { get; set; }
    }
}
