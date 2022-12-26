using BackendAPI.Data;
using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Models.Ranking;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public class RankingRepository : IRankingRepository
    {
        private readonly DatabaseContext _context;
        public RankingRepository(DatabaseContext context)
        {
            _context = context;
        }
        private async Task UpdateUsersRankings()
        {
            List<Ranking> ranking_list = await _context.Rankings.OrderByDescending(cr => cr.MinimumKilometers).ToListAsync();
            foreach (User user in _context.Users)
            {
                double kilometers = user.TravelledKilometers;
                foreach(Ranking r in ranking_list)
                {
                    if (kilometers >= r.MinimumKilometers)
                    {
                        user.Ranking = r;
                        //this is the maximum rank, since it's descending, go to the next user
                        break;
                    }
                }
                _context.User.Update(user);
            }
            await _context.SaveChangesAsync();
        }
        public async Task Create(Ranking ranking)
        {
            
            //não se pode criar um ranking com o mesmo número de quilómetros que outro
            if (await _context.Rankings.AnyAsync(r=>r.MinimumKilometers == ranking.MinimumKilometers))
            {
                throw new CustomException(ErrorType.RANKING_EXISTS);
            }
            if (ranking.MinimumKilometers < 0)
            {
                throw new CustomException(ErrorType.RANKING_INVALID_NUMBER_MINIMUM_KILOMETERS);
            }
            _context.Rankings.Add(ranking);
            await _context.SaveChangesAsync();
            await UpdateUsersRankings();
        }

        public async Task Delete(Ranking ranking)
        {
            if (ranking.MinimumKilometers == 0 || await _context.Rankings.CountAsync()==1)
            {
                throw new CustomException(ErrorType.RANKING_DEFAULT_DELETE);
            }
            _context.Rankings.Remove(ranking);
            await _context.SaveChangesAsync();
            await UpdateUsersRankings();
        }

        public async Task<IEnumerable<Ranking>> GetAll()
        {
            return await _context.Rankings.AsQueryable().ToListAsync();
        }

        public async Task<Ranking> GetById(int Id)
        {
            return await _context.Rankings.Where(r => r.Id == Id).FirstOrDefaultAsync();
        }

        public async Task Update(Ranking ranking, RankingUpdateModel model)
        {
            if (await _context.Rankings.AnyAsync(r => r.MinimumKilometers == model.MinimumKilometers && r.Id != ranking.Id))
            {
                throw new CustomException(ErrorType.RANKING_EXISTS);
            }
            if (model.MinimumKilometers < 0)
            {
                throw new CustomException(ErrorType.RANKING_INVALID_NUMBER_MINIMUM_KILOMETERS);
            }
            ranking.MinimumKilometers = model.MinimumKilometers;
            ranking.Description = model.Description;
            ranking.Name = model.Name;
            ranking.Color = model.Color;
            _context.Rankings.Update(ranking);
            await _context.SaveChangesAsync();
            await UpdateUsersRankings();
        }

        public async Task<Ranking> GetDefaultRanking()
        {
            return await _context.Rankings.Where(r=>r.MinimumKilometers==0).FirstOrDefaultAsync();
        }

        public async Task<Ranking> CreateDefaultRanking()
        {
            Ranking r = new() { Color = "#000000", Description = "Iniciante", MinimumKilometers = 0, Name = "Iniciante" };
            if (await GetDefaultRanking() == null)
            {
                await Create(r);
            }
            return r;
        }
    }
}
