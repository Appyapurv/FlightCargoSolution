using FlightCargoSolution.Interface;
using FlightCargoSolution.Models;
using FlightCargoSolution.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;

namespace FlightCargoSolution.Services
{
    public class CargoService : ICargoInterface
    {
        private readonly IMongoClient _mongoDbClient;
        private readonly IMongoCollection<Cargo> _cargoColl;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ILogger _logger;

        private string lastError = string.Empty;
        public CargoService(IMongoClient mongoClient, ILogger<CargoService> logger)
        {
            _mongoDbClient = mongoClient;
            _mongoDatabase = mongoClient.GetDatabase(Settings.Databases);
            _cargoColl = this._mongoDatabase.GetCollection<Cargo>(Settings.CargoCollectionName).WithWriteConcern(WriteConcern.Acknowledged).WithReadPreference(ReadPreference.SecondaryPreferred);
            _logger = logger;
        }

        public async Task<Cargo> CreateCargo(string location, string destination)
        {
            var cargo = new Cargo
            {
                Received = DateTime.UtcNow,
                Location = location,
                Destination = destination,
                Status = CargoStatus.InProcessStatus,
                SchemaVersion= Settings.SchemaVersion,
            };

            await _cargoColl.InsertOneAsync(cargo);
            return cargo;
        }

        public async Task<bool> CargoDelivered(string id)
        {
            var result = false;
            try
            {
                var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, id);
                var update = Builders<Cargo>.Update
                    .Set(cargo => cargo.Status, CargoStatus.DeliveredStatus);

                var updateCargoResult = await _cargoColl.UpdateOneAsync(filter, update);
                result = updateCargoResult.IsAcknowledged && updateCargoResult.ModifiedCount == 1;
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to update the cargo : {id} with status: {CargoStatus.DeliveredStatus}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
                result = false;
            }
            return result;

        }

        public async Task<Cargo> GetCargoById(string id)
        {
            var filter = new BsonDocument();

            filter[Settings.Id] = id;
            try
            {
                // Will use _id index
                var cursor = await _cargoColl.FindAsync(filter);
                var cargos = cursor.ToList();
                if (cargos.Any())
                {
                    var cargoModel = cargos.FirstOrDefault();
                    return cargoModel;
                }
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to fetch the cargo by the id: {id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
            }

            return null;
        }


        public async Task<bool> UpdateCargo(string id, string callsign)
        {
            var result = false;
            try
            {
                var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, id);
                var update = Builders<Cargo>.Update
                    .Set(cargo => cargo.Courier, callsign);

                var updatedCargoResult = await _cargoColl.UpdateOneAsync(filter, update);

                result = updatedCargoResult.IsAcknowledged;

            }
            catch (MongoException ex)
            {
                lastError = $"Failed to assign courier : {callsign} to the cargo : {id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
                result = false;
            }
            return result;

        }

        public async Task<Cargo> UnloadCargo(string id)
        {
            try
            {
                var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, id);
                var update = Builders<Cargo>.Update
                    .Set(cargo => cargo.Courier, null);

                return await _cargoColl.FindOneAndUpdateAsync(filter, update);
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to assign courier : {id} to the cargo : {id}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
            }
            return null;
        }

        public async Task<Cargo> UpdateCargoLocation(string id, string location)
        {
            try
            {
                var filter = Builders<Cargo>.Filter.Eq(cargo => cargo.Id, id);
                var update = Builders<Cargo>.Update
                    .Set(cargo => cargo.Location, location);

                var result = await _cargoColl.FindOneAndUpdateAsync(filter, update);

                return result;
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to assign courier : {id} to the cargo : {id}.Exception: {ex.ToString()}";
                throw;
            }
            return null;
        }

        public async Task<List<Cargo>> GetCargoAtLocation(string location)
        {
            var cargos = new ConcurrentBag<Cargo>();
            var builder = Builders<Cargo>.Filter;
            var filter = builder.Eq("status", CargoStatus.InProcessStatus) &
                         builder.Eq("location", location);

            try
            {
                // Created index with status, location and courier -> db.cargos.createIndex({status:1,location:1})
                var cursor = await _cargoColl.FindAsync(filter);
                var cargoDtos = cursor.ToList();

                // Parallelizing the serialization to make it faster.
                Parallel.ForEach(cargoDtos, cargoDto =>
                {
                    var cargoModel = cargoDto;
                    cargos.Add(cargoModel);
                });
            }
            catch (MongoException ex)
            {
                lastError = $"Failed to fetch the cargoes at the location: {location}.Exception: {ex.ToString()}";
                _logger.LogError(lastError);
            }

            return cargos.OrderBy(x => x.Id).ToList();

        }
    }
}
