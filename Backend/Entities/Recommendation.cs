using System.Collections.Generic;

namespace BackendAPI.Entities
{
    public class Recommendation
    {
        public List<Group> featuredGroups;
        public List<Group> recommendedGroups;
        public List<Trip> recommendedTrips;
        //ultimos posts da comunidade, incluindo de viagens privadas, caso o user esteja nelas
        public List<Post> latestPostsCommunity;
        //vazio no caso de ser anonimo
        public List<Post> latestPostsFriends;
    }
}
