using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FlightCargoSolution.Models
{
	public class Cargo
	{
		[BsonId]
		[BsonElement("_id")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		[BsonElement("courier")]
		public string Courier { get; set; }
		[BsonElement("received")]
		public DateTime Received { get; set; }
		[BsonElement("status")]
		public string Status { get; set; }
		[BsonElement("location")]
		public string Location { get; set; }
		[BsonElement("destination")]
		public string Destination { get; set; }

		[BsonElement("schemaversion")]
		public string SchemaVersion { get; set; }
	}
}