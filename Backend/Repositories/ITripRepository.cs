using BackendAPI.Entities;
using BackendAPI.Models.Activity;
using BackendAPI.Models.Trip;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetAll();
        Task<Trip> GetById(int Id);
        Task Create(Trip trip, Group group);
        Task Update(Trip trip, TripDetailsUpdateModel model);
        Task Delete(Trip trip);
        Task AddUser(Trip trip, User user, Guid? InviteId, bool IsManager);
        Task RemoveUser(Trip trip, User user);
        Task InviteUser(TripInvite tripInvite);
        Task<IEnumerable<Trip>> Search(TripSearchModel model);
        Task<UserTrip> GetUserTrip(Trip trip, User user);
        Task RecalculateTripDistanceAndBudget(Trip trip);
        Task<TripInvite> GetTripInviteById(Guid? Id);
        Task RemoveInvite(TripInvite invite);
        Task UpdateImage(Trip trip, IFormFile file);
        Task RemoveImage(Trip trip);
    }
}