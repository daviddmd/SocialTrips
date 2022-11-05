using BackendAPI.Entities;
using BackendAPI.Models.Ranking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public interface IRankingRepository
    {
        Task Create(Ranking ranking);
        Task Update(Ranking ranking, RankingUpdateModel model);
        Task Delete(Ranking ranking);
        Task<IEnumerable<Ranking>> GetAll();
        Task<Ranking> GetById(int Id);
        Task<Ranking> GetDefaultRanking();
        Task<Ranking> CreateDefaultRanking();
    }
}
