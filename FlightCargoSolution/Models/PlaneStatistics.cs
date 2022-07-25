using MongoDB.Bson.Serialization.Attributes;

namespace FlightCargoSolution.Models
{
    public class PlaneStatistics
    {

        [BsonElement("distanceTravelledInMiles")]
        public double TotalDistanceTravelledInMiles { get; set; }

        [BsonElement("distanceTravelledSinceLastMaintenanceInMiles")]
        public double DistanceTravelledSinceLastMaintenanceInMiles { get; set; }

        [BsonElement("maintenanceRequired")]
        public bool MaintenanceRequired { get; set; }

        [BsonElement("airtime")]
        public double AirtimeInMinutes { get; set; }

    }
}
