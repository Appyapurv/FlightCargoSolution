using MongoDB.Bson.Serialization.Attributes;

namespace FlightCargoSolution.Models
{
    public class DistanceCalculated
    {
        [BsonElement("distance")]
        public double Distance { get; set; }
    }
}
