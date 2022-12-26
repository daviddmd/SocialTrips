using BackendAPI.Entities;
using BackendAPI.Models.Trip;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Repository for the Trips
    /// </summary>
    public interface ITripRepository
    {
        /// <summary>
        /// Get all Trips in the System
        /// </summary>
        /// <returns>List of Trips in the System</returns>
        Task<IEnumerable<Trip>> GetAll();
        /// <summary>
        /// Get a Trip by its ID
        /// </summary>
        /// <param name="Id">ID of the Trip</param>
        /// <returns>Entity of the Trip</returns>
        Task<Trip> GetById(int Id);
        /// <summary>
        /// Create a Trip associated with a Group
        /// </summary>
        /// <param name="trip">Trip Entity</param>
        /// <param name="group">Group Entity</param>
        /// <returns></returns>
        Task Create(Trip trip, Group group);
        /// <summary>
        /// Update a Trip with new Details associated with its <see cref="TripDetailsUpdateModel"/>, such as its Name, Description, Beginning and Ending Dates, Completed and Visibility Status.
        /// Fails if:
        /// <list type="number">
        /// <item>The updating of the beginning or ending dates cause overlap with existing trip activities</item>
        /// <item>The new beginning date is greater than the new ending date</item>
        /// <item>If the beginning or ending dates differ from the current ones, if the beginning or ending dates are lower than the current date</item>
        /// </list>
        /// A trip finished status is final, meaning a trip, once finished, its status cannot be reverted. 
        /// A finished trip means the itinerary is final, new people cannot join the trip and all associated invites are cleared.
        /// 
        /// </summary>
        /// <param name="trip">Trip to be Updated</param>
        /// <param name="model">Model of the Update Details of the Trip</param>
        /// <returns></returns>
        Task Update(Trip trip, TripDetailsUpdateModel model);
        /// <summary>
        /// Sets the trip visibility status to private, removes all users and invites from it and sets all the associated posts visibility status to hidden.
        /// </summary>
        /// <param name="trip">Trip to Delete</param>
        /// <returns></returns>
        Task Delete(Trip trip);
        /// <summary>
        /// Adds an User to a Trip, with or without an invite (mandatory if private).
        /// Fails if:
        /// <list type="number">
        /// <item>The user is already on the trip</item>
        /// <item>The user isn't in the group associated with the trip</item>
        /// <item>The trip already finished</item>
        /// <item>The trip is private and the user didn't use a valid invite (and isn't a manager)</item>
        /// </list>
        /// </summary>
        /// <param name="trip">Trip</param>
        /// <param name="user">User to be added to a Trip</param>
        /// <param name="InviteId">Invite Guid</param>
        /// <param name="IsManager">Whether or not the user to add is a manager</param>
        /// <returns></returns>
        Task AddUser(Trip trip, User user, Guid? InviteId, bool IsManager);
        /// <summary>
        /// Remove a user from a trip. Fails if the user isn't in the trip.
        /// </summary>
        /// <param name="trip">Trip to remove user from</param>
        /// <param name="user">User to remove from the trip</param>
        /// <returns></returns>
        Task RemoveUser(Trip trip, User user);
        /// <summary>
        /// Invite user to trip. Fails if:
        /// <list type="number">
        /// <item>The trip already finished</item>
        /// <item>The user was already invited to the trip</item>
        /// <item>The user is already in the trip</item>
        /// <item>The user to invite isn't in the group associated with the trip</item>
        /// </list>
        /// </summary>
        /// <param name="tripInvite">Entity containing the Invitation Date, User and Trip</param>
        /// <returns></returns>
        Task InviteUser(TripInvite tripInvite);
        /// <summary>
        /// Search for Existing Trips by Name, Description and Destination, which is a Location in any existing upcoming trip,
        /// that may be referred in any of the trip's names or description or any of each trip's activity's real addresses or descriptions.
        /// </summary>
        /// <param name="model">Trip Search Model that contains the Name, Description and Destination search filters</param>
        /// <returns>List of Trips with matching characteristics to the search terms</returns>
        Task<IEnumerable<Trip>> Search(TripSearchModel model);
        /// <summary>
        /// Gets the User Trip model associated with an existing user in a trip. Fails if an user isn't in a trip.
        /// </summary>
        /// <param name="trip">Trip</param>
        /// <param name="user">User in Trip</param>
        /// <returns></returns>
        Task<UserTrip> GetUserTrip(Trip trip, User user);
        /// <summary>
        /// Re-calculate the Trip Distance and Budget, normally used after adding, updating or removing activities from a trip. Makes use of the coordinates stored in each activity 
        /// to avoid contacting the Google Maps API each call.
        /// </summary>
        /// <param name="trip">Trip to Update Total Distance and Budget</param>
        /// <returns></returns>
        Task RecalculateTripDistanceAndBudget(Trip trip);
        /// <summary>
        /// Get a Trip Invite by its Guid.
        /// </summary>
        /// <param name="Id">Guid of the Trip Invite</param>
        /// <returns>Entity associated with the Trip Invite</returns>
        Task<TripInvite> GetTripInviteById(Guid? Id);
        /// <summary>
        /// Remove a Trip Invite
        /// </summary>
        /// <param name="invite">Trip Invite to Remove</param>
        /// <returns></returns>
        Task RemoveInvite(TripInvite invite);
        /// <summary>
        /// Update the Image of the Trip
        /// </summary>
        /// <param name="trip">Trip to update the Image</param>
        /// <param name="file">Form File containing the new Image of the Trip</param>
        /// <returns></returns>
        Task UpdateImage(Trip trip, IFormFile file);
        /// <summary>
        /// Remove the Trip Image
        /// </summary>
        /// <param name="trip">Trip to remove image from</param>
        /// <returns></returns>
        Task RemoveImage(Trip trip);
    }
}