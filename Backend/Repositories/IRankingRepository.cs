using BackendAPI.Entities;
using BackendAPI.Models.Ranking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Repository for the System's Rankings
    /// </summary>
    public interface IRankingRepository
    {
        /// <summary>
        /// Creates a Ranking. Fails if another ranking exists with the same number of minimum kilometers to obtain.
        /// Upon adding, the ranking of all users may change reflecting the addition of a new one according to their current number of travelled kilometers.
        /// </summary>
        /// <param name="ranking">Ranking Entity to Add</param>
        /// <returns></returns>
        Task Create(Ranking ranking);
        /// <summary>
        /// Updates the Details of a Ranking. Details that may be updated are the Ranking's Description, Color, Name and Minimum Number of Kilometers for an user to obtain. 
        /// Upon updating, the ranking of all users may change reflecting the change of an existing rank, according to their current number of travelled kilometers
        /// and the current maximum rank corresponding to this number. Fails if the new minumum number of kilometers is equal to the number of kilometers of an existing rank.
        /// </summary>
        /// <param name="ranking">Ranking to Update</param>
        /// <param name="model">Ranking Update Model with details to update the rank</param>
        /// <returns></returns>
        Task Update(Ranking ranking, RankingUpdateModel model);
        /// <summary>
        /// Delete a rank. The default rank (With minimum distance of 0 kilometers) may not be deleted.
        /// </summary>
        /// <param name="ranking">Rank to delete</param>
        /// <returns></returns>
        Task Delete(Ranking ranking);
        /// <summary>
        /// Get all ranks in the system.
        /// </summary>
        /// <returns>List of ranks in the system</returns>
        Task<IEnumerable<Ranking>> GetAll();
        /// <summary>
        /// Get a rank by its ID.
        /// </summary>
        /// <param name="Id">ID of the rank</param>
        /// <returns>Entity of the Rank</returns>
        Task<Ranking> GetById(int Id);
        /// <summary>
        /// Get the default Rank of the System (with associated minimum distance of 0 kilometers).
        /// </summary>
        /// <returns>Entity associated with the default rank.</returns>
        Task<Ranking> GetDefaultRanking();
        /// <summary>
        /// Create the default ranking with minimum distance of 0 kilometers.
        /// </summary>
        /// <returns>Entity representing the Default Ranking</returns>
        Task<Ranking> CreateDefaultRanking();
    }
}
