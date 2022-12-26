using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Information Repository, for providing statistics for the application's users in their dashboards and managers in the statistics dashboard.
    /// </summary>
    public interface IInformationRespository
    {
        /// <summary>
        /// Gets Trips and Groups Recommendations for an User
        /// </summary>
        /// <param name="user">User to fetch recommendations</param>
        /// <returns><see cref="Recommendation"/>, with:
        /// <list type="number">
        /// <item>List of Featured Groups (that the user isn't in)</item>
        /// <item>List of Recommended Groups (that the user isn't in)</item>
        /// <item>List of Recommended Trips (that the user isn't in)</item>
        /// <item>List of Latest Community Posts</item>
        /// <item>List of Latest Posts of the People the User Follows (Empty if the User Isn't Logged In)</item>
        /// </list>
        /// </returns>
        Task<Recommendation> GetRecommendations(User user);
        /// <summary>
        /// Gets Platform Statistics.
        /// </summary>
        /// <returns>
        /// <see cref="Statistic"/>, with:
        /// <list type="number">
        /// <item>Dictionary with Most Visited Places (with number of visits for each one)</item>
        /// <item>Dictionary with Trips by Total Distance</item>
        /// <item>Dictionary with Trips by Total Cost</item>
        /// <item>Dictionary with Trips by Number of Users</item>
        /// <item>Dictionary with Groups by Average Combinated Trip Distance</item>
        /// <item>Dictionary with Groups by Average Trip Cost</item>
        /// <item>Dictionary with Groups by Number of Members</item>
        /// <item>Dictionary with the Distribution of the User Ranks</item>
        /// <item>Dictionary with the Distribution of the <see cref="TransportType">Transport Types</see></item>
        /// <item>Dictionary with the Distribution of the <see cref="ActivityType">Activity Types</see></item>
        /// </list>
        /// </returns>
        Task<Statistic> GetStatistics();
    }
}
