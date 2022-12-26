using BackendAPI.Entities.Enums;
using BackendAPI.Models.Activity;
using GoogleApi;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Common.Enums;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using GoogleApi.Entities.Common.Enums.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geolocation;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using GoogleApi.Entities.Places.Details.Request;
using Microsoft.Extensions.Configuration;
using GoogleApi.Entities.Maps.DistanceMatrix.Response;
using GoogleApi.Entities.Places.Details.Response;
using BackendAPI.Exceptions;
using Ganss.Xss;

namespace BackendAPI.Helpers
{
    public class GoogleMapsHelper : IGoogleMapsHelper
    {
        private readonly IConfiguration _configuration;
        private readonly string ApiKey;
        public GoogleMapsHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            ApiKey = _configuration["Google:ApiKey"];
        }

        //ter em consideração o other, usar o distancematrix para esse...ou usar o distancematrix para todos, avaliar
        //podemos futuramente usar isto para usar a distance matrix api para calcular a distância entre 2 pontos tendo em conta 
        public async Task<int> CalculateDistanceBetweenTwoPoints(string OriginPlaceId, string DestinationPlaceId, TravelMode travelMode)
        {
            Place origin = new Place(OriginPlaceId);
            Place destination = new Place(DestinationPlaceId);
            DistanceMatrixRequest request = new DistanceMatrixRequest
            {
                Key = ApiKey,
                Origins = new[]{
                    new LocationEx(origin)
                },
                Destinations = new[]{
                    new LocationEx(destination)
                },
                TravelMode = travelMode
            };
            DistanceMatrixResponse response = await GoogleMaps.DistanceMatrix.QueryAsync(request);
            foreach (Row r in response.Rows)
            {
                foreach (Element e in r.Elements)
                {
                    return e.Distance.Value;
                }
            }
            return 0;
        }

        public async Task<IEnumerable<ActivityTransportModel>> GetAllTransporationMethods(ActivitySearchTransportModel model)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedCssProperties.Clear();
            DateTime departTime = model.DepartTime < DateTime.Now ? DateTime.Now : model.DepartTime;
            List<ActivityTransportModel> result = new();
            Place origin = new(model.OriginPlaceId);
            Place destination = new(model.DestinationPlaceId);
            string CountryCode = model.CountryCode ?? "PT";
            foreach (TravelMode travelMode in Enum.GetValues(typeof(TravelMode)))
            {
                DirectionsRequest request = new()
                {
                    Key = ApiKey,
                    Origin = new LocationEx(origin),
                    Destination = new LocationEx(destination),
                    DepartureTime = departTime,
                    TravelMode = travelMode,
                    Language = (GoogleApi.Entities.Common.Enums.Language)StringExtension.FromCode(CountryCode.ToLower())
                };
                DirectionsResponse response = await GoogleMaps.Directions.QueryAsync(request);
                if (response.Status != GoogleApi.Entities.Common.Enums.Status.Ok || !response.Routes.Any())
                {
                    continue;
                }
                Route route = response.Routes.First();
                if (!route.Legs.Any())
                {
                    continue;
                }
                Leg leg = route.Legs.First();
                string Description = "<ol>\n";
                //construir lista de passos para a descrição do transporte
                foreach (Step step in leg.Steps)
                {
                    if (step.TransitDetails != null)
                    {
                        Description += $"<li>{step.HtmlInstructions}: <b>{step.TransitDetails.DepartureStop.Name}</b>-><b>{step.TransitDetails.ArrivalStop.Name}</b> ({step.TransitDetails.Lines.ShortName})</li>\n";
                    }
                    else
                    {
                        Description += $"<li>{step.HtmlInstructions}</li>\n";
                    }
                }
                Description += "</ol>\n";
                Description = sanitizer.Sanitize(Description);
                DateTime DepartureTime = departTime;
                DateTime ArrivalTime = DepartureTime.AddSeconds(leg.Duration.Value);
                if (travelMode == TravelMode.Transit && leg.DepartureTime!=null)
                {
                    DepartureTime = DateTimeOffset.FromUnixTimeSeconds(leg.DepartureTime.Value).DateTime;
                    ArrivalTime = DateTimeOffset.FromUnixTimeSeconds(leg.ArrivalTime.Value).DateTime;
                }
                int Distance = leg.Distance.Value;
                result.Add(new ActivityTransportModel { ArrivalTime = ArrivalTime, DepartureTime = DepartureTime, Description = Description, Distance = Distance, TransportType = (TransportType)travelMode });
            }
            return result;
        }

        public async Task<(Coordinate,String,String)> GetCoordinatesAndAddressFromPlaceId(string PlaceId)
        {
            PlacesDetailsRequest request = new PlacesDetailsRequest
            {
                Key=ApiKey,
                PlaceId = PlaceId,
            };
            PlacesDetailsResponse response = await GooglePlaces.Details.QueryAsync(request);
            if (response.Status == GoogleApi.Entities.Common.Enums.Status.Ok && response.Result != null)
            {
                return (new Coordinate(response.Result.Geometry.Location.Latitude, response.Result.Geometry.Location.Longitude), response.Result.FormattedAddress, response.Result.Name);

            }
            throw new CustomException("No such place with this Place ID", ErrorType.ACTIVITY_PLACE_ID_NONEXIST);
        }
    }
}
