using BackendAPI.Entities;
using BackendAPI.Entities.Enums;
using BackendAPI.Exceptions;
using BackendAPI.Models.Activity;
using System.Threading.Tasks;

namespace BackendAPI.Repositories
{
    /// <summary>
    /// Repository for the Activity Instances
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Adds an <see cref="Activity"/> to a <see cref="Trip"/> with its respective Transport (also an <see cref="Activity"/>)
        /// <para></para>
        /// Conditions to add an Activity to a Trip:
        /// <list type="number">
        /// <item>An Activity may not be added to a finished trip (as in, finished, with a final itinerary)</item>
        /// <item>The beginning and ending dates of the activity and its respective transport must be in the same day</item>
        /// <item>The beginning date of each activity must be after its respective transport activity</item>
        /// <item>Each activity's beginning and ending dates must be different, and the ending date must be greater than the beginning date</item>
        /// <item>The transport activity may only be non-existant (null) if there are no other activities in a given day (of the activity to add) of the trip</item>
        /// <item>Activities may only be appended to the end of the daily itinerary, like a queue. The first activities to be removed are always the last to be added.</item>
        /// <item>Activities may not have an invalid Place ID (as each activity will be associated with the coordinates of its respective Place ID in order to avoid repeated 
        /// API requests to recalculate the trip's total distance each time a new activity is added or removed)</item>
        /// <item>The Activity to add may not be the same (equal Place ID) to the previous</item>
        /// <item>The Activity to add must have its beginning and ending date in the range of the trip's beginning and ending date in order to fit within its itinerary</item>
        /// <item>An Activity must have a beginning and ending date, even if it's the only activity of a day's itinerary</item>
        /// <item>The Activity's <see cref="TransportType"/> must be valid</item>
        /// </list>
        /// </summary>
        /// <param name="trip">Trip of the Activity</param>
        /// <param name="activity">Activity to Add to the Trip (to a specific day of the trip's itinerary)</param>
        /// <param name="transport">Transport Activity to connect the previous activity of the trip (usually in the same day) to the one to currently add</param>
        /// <returns></returns>
        /// <exception cref="CustomException">Thrown if any of the aformentioned conditions are violated with the respective identifier and message
        /// </exception>
        Task AddActivity(Trip trip, Activity activity, Activity transport);
        /// <summary>
        /// Removes an <see cref="Activity"/> from a <see cref="Trip"/>, possibly alongside its respective Transport <see cref="Activity"/>,
        /// if the respective activity isn't the first activity of the day it's located in
        /// <para></para>
        /// Conditions to remove an Activity from a Trip:
        /// <list type="number">
        /// <item>Activities may not be removed from a finished trip</item>
        /// <item>Activities may not be removed if any suceed it, or if it isn't the last activity in the itinerary of the day it's inserted in</item>
        /// <item>The Transport Activity that comes immediately before it (connecting it to the previous activity) will also be removed</item>
        /// </list>
        /// </summary>
        /// <param name="trip">Trip of the Activity</param>
        /// <param name="activity">Activity to Remove from the Trip</param>
        /// <returns></returns>
        Task RemoveActivity(Trip trip, Activity activity);
        /// <summary>
        /// Returns an Activity by its ID
        /// </summary>
        /// <param name="Id">ID of the Trip</param>
        /// <returns>Trip Entity</returns>
        Task<Activity> GetById(int Id);
        /// <summary>
        /// Updates an <see cref="Activity"/> with its new details with a <see cref="ActivityUpdateModel"/>
        /// <para></para>
        /// Conditions to update an Activity:
        /// <list type="number">
        /// <item>An Activity from a Completed Trip may not be Updated</item>
        /// <item>The Activity's day may not be changed</item>
        /// <item>The Activity's beginning and ending dates may only be changed if such dates don't overlap with the ending date of its previous activity and the beginning date of the next activity respectively</item>
        /// <item>The <see cref="ActivityType"/> of a normal Activity must not be changed to Transport, as well as the Activity Type of a Transport Activity</item>
        /// <item>The real location of an activity (coordinates and place ID) may not be changed, only its user-inputed address and description.</item>
        /// </list>
        /// </summary>
        /// <param name="activity">Activity to Update</param>
        /// <param name="model">Activity Update Model with the Details of the Trip (which may differ from the current details, updating them)</param>
        /// <returns></returns>
        Task UpdateActivity(Activity activity, ActivityUpdateModel model);
    }
}
