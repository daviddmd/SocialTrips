﻿using BackendAPI.Entities.Enums;
using BackendAPI.Models.Trip;
using System;

namespace BackendAPI.Models.Activity
{
    public class ActivityModel
    {
        public int Id { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string GooglePlaceId { get; set; }
        public double ExpectedBudget { get; set; }
        public TripModelSimple Trip { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ActivityType ActivityType { get; set; }
        public TransportType TransportType { get; set; }
    }
}
