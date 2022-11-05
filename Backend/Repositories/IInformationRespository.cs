using BackendAPI.Entities;
using BackendAPI.Models.Information;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public interface IInformationRespository
    {
        Task<Recommendation> GetRecommendations(User user);
        Task<Statistic> GetStatistics();
    }
}
