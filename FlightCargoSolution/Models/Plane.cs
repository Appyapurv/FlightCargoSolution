using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace FlightCargoSolution.Models
{
	public class Plane
	{
		[BsonElement("_id")]
		public string Callsign { get; set; }
		[BsonElement("currentLocation")]

		// public GeoJson2DCoordinates CurrentLocation { get; set; }
		public double[] CurrentLocation { get; set; }
		[BsonElement("heading")]
		public double Heading { get; set; }
		[BsonElement("route")]
		public string[] Route { get; set; }
		[BsonElement("landed")]
		public string Landed { get; set; }
		[BsonElement("distanceTravelledSinceLastMaintenance")]
		public double DistanceTravelledSinceLastMaintenance { get; set; }
		[BsonElement("totalDistanceTravelled")]
		public double TotalDistanceTravelled { get; set; }
		[BsonElement("maintenanceRequired")]
		public bool MaintenanceRequired { get; set; }
		[BsonElement("planeStartedAt")]
		public DateTime PlaneStartedAt { get; set; }
		[BsonElement("travelledinSeconds")]
		public double TravelledinSeconds { get; set; }
		[BsonElement("previousLanded")]
		public string PreviousLanded { get; set; }

		[BsonElement("landedOn")]
		public DateTime LandedOn { get; set; }
		[BsonElement("statistics")]
		public PlaneStatistics Statistics { get; set; }
	}
}