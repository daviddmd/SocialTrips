using AutoMapper;
using BackendAPI.Data;
using BackendAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public class InformationRepository : IInformationRespository
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;

        public InformationRepository(DatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Recommendation> GetRecommendations(User user)
        {
            Recommendation recommendation = new();
            if (user == null)
            {
                recommendation.latestPostsFriends = new List<Post>();
                recommendation.latestPostsCommunity = await _context.Posts.Where(p => !p.IsHidden && !p.Trip.IsPrivate && !p.Trip.Group.IsPrivate).OrderByDescending(p => p.PublishedDate).Take(25).ToListAsync();
                recommendation.featuredGroups = await _context.Groups.Where(g => g.IsFeatured && !g.IsPrivate).
                    OrderByDescending(g => g.Users.Count).
                    ThenByDescending(g => g.CreationDate).
                    Take(12).ToListAsync();
                recommendation.recommendedGroups = await _context.Groups.OrderByDescending(g => g.Users.Count).ThenByDescending(g => g.CreationDate).Take(12).ToListAsync();
                recommendation.recommendedTrips = await _context.Trips.Where(t => !t.IsPrivate && !t.Group.IsPrivate).OrderByDescending(t => t.Users.Count).ThenBy(t => t.BeginningDate).Take(12).ToListAsync();
            }
            else
            {
                recommendation.latestPostsFriends = await _context.Posts.Where(p =>
                user.Following.Contains(p.User) &&
                !p.IsHidden &&
                !(p.Trip.IsPrivate && !p.Trip.Users.Any(ut => ut.User == user)) &&
                !(p.Trip.Group.IsPrivate && !p.Trip.Group.Users.Any(ug => ug.User == user))
                ).OrderByDescending(p => p.PublishedDate).Take(25).ToListAsync();
                recommendation.latestPostsCommunity = await _context.Posts.Where(p =>
                !p.IsHidden &&
                !(p.User == user) &&
                !(p.Trip.IsPrivate && !p.Trip.Users.Any(ut => ut.User == user)) &&
                !(p.Trip.Group.IsPrivate && !p.Trip.Group.Users.Any(ug => ug.User == user))
                ).OrderByDescending(p => p.PublishedDate).Take(25).ToListAsync();
                recommendation.featuredGroups = await _context.Groups.Where(g =>
                g.IsFeatured &&
                !g.Users.Any(ug => ug.User == user) && !g.IsPrivate
                ).OrderByDescending(g => g.Users.Count).ThenByDescending(g => g.CreationDate).Take(12).ToListAsync();
                recommendation.recommendedGroups = await _context.Groups.Where(g =>
                !g.Users.Any(u => u.User == user)
                ).OrderByDescending(g => g.Users.Count).ThenByDescending(g => g.CreationDate).Take(12).ToListAsync();
                recommendation.recommendedTrips = await _context.Trips.Where(t =>
                !t.IsPrivate &&
                !(t.Group.IsPrivate && !t.Group.Users.Any(ug => ug.User == user)) &&
                !t.Users.Any(ut => ut.User == user) &&
                !t.IsCompleted
                ).OrderByDescending(t => t.Users.Count).ThenBy(t => t.BeginningDate).Take(12).ToListAsync();
            }

            return recommendation;
        }

        public async Task<Statistic> GetStatistics()
        {
            Statistic statistic = new();
            statistic.GroupsByNumberUsers = await _context.Groups.OrderByDescending(g => g.Users.Count).Take(10).ToDictionaryAsync(g => g.Name, g => g.Users.Count);
            statistic.TripsByNumberUsers = await _context.Trips.OrderByDescending(t => t.Users.Count).Take(10).ToDictionaryAsync(t => t.Name, t => t.Users.Count);
            statistic.MostVisitedPlaces = await _context.Activities.Where(a => a.ActivityType != Entities.Enums.ActivityType.TRANSPORT && a.RealAddress != null).GroupBy(a => a.RealAddress).Select(a => new { a.Key, Count = a.Count() }).OrderByDescending(a => a.Count).Take(10).ToDictionaryAsync(a => a.Key, a => a.Count);
            statistic.GroupsByAverageDistance = await _context.Groups.OrderByDescending(g => g.AverageTripDistance).Take(10).ToDictionaryAsync(g => g.Name, g => g.AverageTripDistance);
            statistic.GroupsByAverageCost = await _context.Groups.OrderByDescending(g => g.AverageTripCost).Take(10).ToDictionaryAsync(g => g.Name, g => g.AverageTripCost);
            statistic.TripsByTotalDistance = await _context.Trips.OrderByDescending(t => t.TotalDistance).Take(10).ToDictionaryAsync(t => t.Name, t => t.TotalDistance);
            statistic.TripsByTotalCost = await _context.Trips.OrderByDescending(t => t.ExpectedBudget).Take(10).ToDictionaryAsync(t => t.Name, t => t.ExpectedBudget);
            statistic.RankingUserDistribution = await _context.User.GroupBy(u => u.Ranking.Name).Select(u => new { u.Key, Count = u.Count() }).OrderByDescending(u => u.Count).Take(10).ToDictionaryAsync(u => u.Key, u => u.Count);
            statistic.TransportTypeDistribution = await _context.Activities.Where(a => a.ActivityType == Entities.Enums.ActivityType.TRANSPORT).GroupBy(a => a.TransportType).Select(a => new { a.Key, Count = a.Count() }).OrderByDescending(a => a.Count).Take(10).ToDictionaryAsync(a => a.Key, a => a.Count);
            statistic.ActivityTypeDistribution = await _context.Activities.Where(a => a.ActivityType != Entities.Enums.ActivityType.TRANSPORT).GroupBy(a => a.ActivityType).Select(a => new { a.Key, Count = a.Count() }).OrderByDescending(a => a.Count).Take(10).ToDictionaryAsync(a => a.Key, a => a.Count);

            return statistic;

        }
    }
}
