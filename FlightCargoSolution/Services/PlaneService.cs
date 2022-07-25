using FlightCargoSolution.Interface;
using FlightCargoSolution.Models;
using FlightCargoSolution.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace FlightCargoSolution.Services
{
    public class PlaneService : IPlanesInterface
    {
        private readonly IMongoClient _mongoDbClient;
        private readonly IMongoCollection<Plane> planesCollection;
        private readonly IMongoDatabase _mongoDatabase;
        private string lastError = string.Empty;
        private bool isTimeSet = false;
        private readonly ILogger _logger;

        public PlaneService(IMongoClient mongoClient, ILogger<PlaneService> logger)
        {
            this._mongoDbClient = mongoClient;
            this._mongoDatabase = mongoClient.GetDatabase(Settings.Databases);
            this.planesCollection = this._mongoDatabase.GetCollection<Plane>(Settings.PlanesCollectionName).WithWriteConcern(WriteConcern.Acknowledged).WithReadPreference(ReadPreference.SecondaryPreferred);
            _logger = logger;
        }

        public async Task<IEnumerable<Plane>> GetAllPlanes()
        {
            var sort = Builders<Plane>.Sort.Ascending(Settings.Id);
            var findOptions = new FindOptions<Plane, BsonDocument>()
            {
                // Sort is to display the city names in order 
                Sort = sort
            };
            //  _id is index
            var planeDtosCursor = await this.planesCollection.FindAsync(new BsonDocument(), findOptions);
            var planeDtos = planeDtosCursor.ToList();
            var planes = new ConcurrentBag<Plane>();

            // Parallelizing the serialization to make it faster.
            Parallel.ForEach(planeDtos, planeDto =>
            {
                var planeModel = BsonSerializer.Deserialize<Plane>(planeDto);
                planes.Add(planeModel);
            });

            return planes.ToList();
        }

        // get plane by Id
        public async Task<Plane> GetPlaneById(string Id)
        {
            var filter = new BsonDocument();

            filter[Settings.Id] = Id;
            try
            {
                // _id is index
                var cursor = await this.planesCollection.FindAsync(filter);
                var planes = cursor.ToList();
                if (planes.Any())
                {
                    var plane = planes.FirstOrDefault();
                    return plane;
                }

            }
            catch (MongoException ex)
            {
                lastError = $"Failed to fetch the plane by id: {Id} Exception: {ex.ToString()}";
                _logger.LogError(lastError);
            }

            return null;
        }


        public async Task<Plane> MovePlaneLocation(string id, string location, int heading)
        {

            var plane = await this.planesCollection.Find(plane => plane.Callsign == id).FirstOrDefaultAsync();

            double travelledDistance = DistanceTo(location.Split(",").Select(x => double.Parse(x)).ToArray(), id);
            double distanceTravelledSinceLastMaintenance = 0;
            bool maintenanceRequired = false;
            double totalSecond = 0;
            if (plane.PlaneStartedAt != default(DateTime))
            {
                totalSecond = CalculateTime(plane.PlaneStartedAt);
            }

            if (!plane.MaintenanceRequired)
            {
                distanceTravelledSinceLastMaintenance = plane.DistanceTravelledSinceLastMaintenance + travelledDistance;
                maintenanceRequired = distanceTravelledSinceLastMaintenance > 50000;
            }


            var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, id);

            var update = Builders<Plane>.Update
               .Set(plane => plane.CurrentLocation, location.Split(",").Select(x => double.Parse(x)).ToArray())
               .Set(plane => plane.Heading, heading)
               .Set(plane => plane.DistanceTravelledSinceLastMaintenance, distanceTravelledSinceLastMaintenance)
               .Set(plane => plane.MaintenanceRequired, maintenanceRequired)
               .Set(plane => plane.TotalDistanceTravelled, plane.TotalDistanceTravelled + travelledDistance)
                .Set(plane => plane.TravelledinSeconds, totalSecond);

            if (plane.PlaneStartedAt == default(DateTime) && !isTimeSet)
            {
                update = Builders<Plane>.Update
           .Set(plane => plane.CurrentLocation, location.Split(",").Select(x => double.Parse(x)).ToArray())
           .Set(plane => plane.Heading, heading)
           .Set(plane => plane.DistanceTravelledSinceLastMaintenance, distanceTravelledSinceLastMaintenance)
           .Set(plane => plane.MaintenanceRequired, maintenanceRequired)
           .Set(plane => plane.TotalDistanceTravelled, plane.TotalDistanceTravelled + travelledDistance)
           .Set(plane => plane.PlaneStartedAt, DateTime.UtcNow)
          .Set(plane => plane.TravelledinSeconds, totalSecond);

                isTimeSet = true;
            }

            var result = await this.planesCollection.FindOneAndUpdateAsync(filter, update);
            return result;
        }

        // calculated distance using geoNear Aggregation
        private double DistanceTo(double[] currentLocation, string Id)
        {
            PipelineDefinition<Plane, DistanceCalculated> distanceQuery = new BsonDocument[]
        {
            new BsonDocument("$geoNear",
            new BsonDocument
                {
                    { "near",
            new BsonDocument
                    {
                        { "type", "Point" },
                        { "coordinates",
            new BsonArray
                        {
                            currentLocation[0],
                            currentLocation[1]
                        } }
                    } },
                    { "distanceField", "distance" },
                    { "query",
            new BsonDocument("_id", "CARGO0") },
                    { "distanceMultiplier", 0.001 },
                    { "spherical", true }
                }),
            new BsonDocument("$project",
            new BsonDocument
                {
                    { "distance", 1 },
                    { "_id", 0 }
                })
        };
            try
            {
                var data = this._mongoDatabase.GetCollection<Plane>("planes").Aggregate(distanceQuery).FirstOrDefault();
                return data.Distance;

            }
            catch (Exception ex)
            {
                lastError = $"Failed to get neareset distance : {Id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
                return 0;
            }

        }

        private double CalculateTime(DateTime planeStartedAt)
        {
            var time = planeStartedAt.ToUniversalTime();
            var currentTime = DateTime.UtcNow;
            return (currentTime - time).TotalSeconds;
        }

        public async Task<bool> AddDestination(string id, string city)
        {
            var result = false;
            try
            {
                var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, id);
                var update = Builders<Plane>.Update
                .Set(plane => plane.Route, new string[] { city });

                var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
                result = updatedPlaneResult.IsAcknowledged;
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to add plane route : {city} for the plane: {id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
                result = false;
            }
            return result;
        }
        public string GetLastError()
        {
            return lastError;
        }

        public async Task<bool> UpdateDestination(string id, string city)

        {
            var result = false;
            try
            {
                var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, id);
                var update = Builders<Plane>.Update
                    .AddToSet(plane => plane.Route, city);

                var updatedPlaneResult = await this.planesCollection.UpdateOneAsync(filter, update);
                result = updatedPlaneResult.IsAcknowledged;

            }
            catch (MongoException ex)
            {
                lastError = $"Failed to replace plane route : {city} for the plane: {id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
                result = false;
            }
            return result;
        }

        public async Task<bool> RemoveDestination(string id)
        {
            var result = false;
            try
            {
                var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, id);

                var filterPlaneId = Builders<Plane>.Filter.Eq(plane => plane.Callsign, id);
                var plane = await this.planesCollection.Find(filterPlaneId).FirstOrDefaultAsync();

                var previouslyLanded = string.Empty;

                if (plane?.Route?.Length > 0)
                {
                    previouslyLanded = plane.Route.First();
                }

                var update = Builders<Plane>.Update
                                            .Set(plane => plane.PreviousLanded, previouslyLanded)
                                            .PopFirst(p => p.Route);

                var updResult = await planesCollection.UpdateOneAsync(filter, update);
                result = updResult.IsAcknowledged;
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to remove the first route  for the plane: {id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
                result = false;
            }
            return result;
        }

        public async Task<Plane> UpdateLandPlaneLocation(string id, string location, int heading, string city)
        {
            var plane = await this.planesCollection.Find(plane => plane.Callsign == id).FirstOrDefaultAsync();
            var previousLocation = plane.CurrentLocation;
            var newLocation = location.Split(",").Select(x => double.Parse(x)).ToArray();

            bool maintenanceRequired = false;
            double totalSecond = CalculateTime(plane.PlaneStartedAt);

            var filter = Builders<Plane>.Filter.Eq(plane => plane.Callsign, id);

            var update = Builders<Plane>.Update
                .Set(plane => plane.CurrentLocation, newLocation)
                .Set(plane => plane.Heading, heading)
                .Set(plane => plane.Landed, city)
                .Set(plane => plane.MaintenanceRequired, maintenanceRequired)
                .Set(plane => plane.TravelledinSeconds, totalSecond);


            var result = await this.planesCollection.FindOneAndUpdateAsync(filter, update);

            return result;
        }
    }
}
