using System.Threading.Tasks;
using System;
using Geolocation;
using BackendAPI.Models.Activity;
using System.Collections.Generic;

namespace BackendAPI.Helpers
{
    public interface IGoogleMapsHelper
    {
        Task<(Coordinate, String, String)> GetCoordinatesAndAddressFromPlaceId(string PlaceId);
        Task<IEnumerable<ActivityTransportModel>> GetAllTransporationMethods(ActivitySearchTransportModel model);
    }
}
