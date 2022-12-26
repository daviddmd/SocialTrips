using System.Threading.Tasks;
using System;
using Geolocation;
using BackendAPI.Models.Activity;
using System.Collections.Generic;
using BackendAPI.Entities.Enums;

namespace BackendAPI.Helpers
{
    /// <summary>
    /// Helper for Google Maps APIs.
    /// </summary>
    public interface IGoogleMapsHelper
    {
        /// <summary>
        /// Helper method to get the Coordinates, Formatted Address and Place Name from a Google Maps Place ID.
        /// </summary>
        /// <param name="PlaceId">Google Maps Place ID</param>
        /// <returns>Tuple with the Place's Coordinates, Formatted Address and Name respectively</returns>
        Task<(Coordinate, String, String)> GetCoordinatesAndAddressFromPlaceId(string PlaceId);
        /// <summary>
        /// Helper method to get all Transportation Methods between two places, given a specific departure time, Origin and Destination Google Maps Place IDs and the Country Code
        /// of the user for the resulting transportation results instructions language. If the transport type is of Transit (Bus, Train...) the Departure Time may be adjusted to fit with the nearest departure.
        /// </summary>
        /// <param name="model"><see cref="ActivitySearchTransportModel">ActivitySearchTransportModel</see> with Departure Time, Origin and Destination Google Maps Place IDs and the Country Code</param>
        /// <returns>List of <see cref="ActivityTransportModel">ActivityTransportModel</see> with each one having a <see cref="TransportType">Transport Type</see> (Transit, Car, Bike or by Foot), Distance
        /// in meters, Departure Time, Arrival Time and a sanitized HTML string Description with the Google Maps transport instructions that may be editable by the user.
        /// </returns>
        Task<IEnumerable<ActivityTransportModel>> GetAllTransporationMethods(ActivitySearchTransportModel model);
    }
}
