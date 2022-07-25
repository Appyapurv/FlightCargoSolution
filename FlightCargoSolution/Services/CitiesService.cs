
using FlightCargoSolution.Interface;
using FlightCargoSolution.Models;
using FlightCargoSolution.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

using System.Collections.Concurrent;

namespace FlightCargoSolution.Services
{
    public class CitiesService : ICitiesInterface
    {
        private readonly IMongoClient _mongoDbClient;
        private readonly IMongoCollection<BsonDocument> citiesCollection;
        private readonly IMongoDatabase mongoDatabase;
        private readonly ILogger _logger;
        private string lastError = string.Empty;
        private double maxDistance = 100000000000;

        public CitiesService(IMongoClient mongoDbClient, ILogger<CitiesService> logger)
        {
            this._mongoDbClient = mongoDbClient;
            this.mongoDatabase = mongoDbClient.GetDatabase(Settings.Databases);
            this._logger = logger;
            this.citiesCollection = this.mongoDatabase.GetCollection<BsonDocument>(Settings.CityCollectionName).WithWriteConcern(WriteConcern.Acknowledged).WithReadPreference(ReadPreference.SecondaryPreferred);
        }

        /// <summary>
        /// GetAll Cities
        /// </summary>
        /// <returns>List of cities</returns>
        public async Task<IEnumerable<City>> GetAllCities()
        {
            var sort = Builders<BsonDocument>.Sort.Ascending(Settings.Id);
            var findOptions = new FindOptions<BsonDocument, BsonDocument>()
            {
                // Sort is to display the city names in order in the front end
                Sort = sort
            };
            //  _id is indexed
            var cityDtosCursor = await this.citiesCollection.FindAsync(new BsonDocument(), findOptions);
            var cityDtos = cityDtosCursor.ToList();
            var cities = new ConcurrentBag<City>();
            // Parallelizing the serialization to make it faster.
            Parallel.ForEach(cityDtos, cityDto =>
            {
                var cityModel = BsonSerializer.Deserialize<City>(cityDto);
                cities.Add(cityModel);
            });

            return cities.ToList();
        }
        /// <summary>
        /// get city by city name
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public async Task<City> GetCityByName(string cityName)
        {
            var filter = new BsonDocument();

            filter[Settings.Id] = cityName;
            try
            {
                // _id is indexed
                var cursor = await this.citiesCollection.FindAsync(filter);
                var cities = cursor.ToList();
                if (cities.Any())
                {
                    var cityModel = BsonSerializer.Deserialize<City>(cities.FirstOrDefault());
                    return cityModel;
                }

            }
            catch (MongoException ex)
            {
                lastError = $"Failed to fetch the city by id: {cityName} Exception: {ex.ToString()}";
                _logger.LogError(lastError);
            }

            return null;
        }
        /// <summary>
        /// GetNeighbouringCities
        /// </summary>
        /// <param name="cityName"></param>
        /// <param name="count"></param>
        /// <returns>list of Get Neighbouring Cities</returns>
        public async Task<dynamic> GetNeighbouringCities(string cityName, long count)
        {
            var collection = this.mongoDatabase.GetCollection<City>("cities");

            var cities = await collection.Find(city => city.Name == cityName).ToListAsync();

            if (cities.Any())
            {
                var city = cities.First();
                var point = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(city.Location[0], city.Location[1]));

                var filter = Builders<City>.Filter.Near(x => x.Location, point, maxDistance, 0);
                var nearbyCities = await collection.Find(filter).ToListAsync();

                var selectedCities = nearbyCities.Take((int)count).ToList();

                return new
                {
                    Neighbors = selectedCities.Select(x => new
                    {
                        Name = x.Name,
                        Country = x.Country,
                        Location = new double[] { x.Location[0], x.Location[1] }
                    })
                };
            }

            return new
            {
                Neighbors = new List<City> { }
            };
        }
        public string GetLastError()
        {
            return lastError;
        }
    }
}
