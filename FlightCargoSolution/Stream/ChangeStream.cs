
using FlightCargoSolution.Models;
using FlightCargoSolution.Utils;
using MongoDB.Driver;

namespace FlightCargoSolution.ChangeStream
{
    public static class ChangeStream
    {
        public static async Task Monitor(IMongoClient client)
        {
            var _database = client.GetDatabase(Settings.Databases);
            var planes = _database.GetCollection<Plane>(Settings.PlanesCollectionName);
            var cities = _database.GetCollection<City>(Settings.CargoCollectionName);

            try
            {
                var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
                var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Plane>>().Match("{ operationType: { $in: [ 'update'] }, 'updateDescription.updatedFields.landed' : { $exists: true } }");

                using (var cursor = await planes.WatchAsync(pipeline, options))
                {
                    await cursor.ForEachAsync(change =>
                    {
                        var document = change.FullDocument;

                        if (!string.IsNullOrWhiteSpace(document.Landed) && !string.IsNullOrWhiteSpace(document.PreviousLanded))
                        {
                            var travelledCitiesNames = new string[] { document.Landed, document.PreviousLanded };
                            var filter = Builders<City>.Filter.In(city => city.Name, travelledCitiesNames);

                            var travelledCities = cities.Find(filter).ToList();

                            UpdateDocument(document, travelledCities);
                        }
                    });
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void UpdateDocument(Plane document, List<City> travelledCities)
        {
            if (travelledCities.Count != 2)
            {
                return;
            }

            var travelledDistance = Extensions.GetDistance(travelledCities[0].Location, travelledCities[1].Location);
            var timeTaken = (DateTime.UtcNow - document.LandedOn).TotalMinutes;


            // Check if maintenance is required
            double distanceTravelledSinceLastMaintenance = 0;
            bool maintenanceRequired = false;

            if (document.Statistics?.MaintenanceRequired ?? false == false)
            {
                distanceTravelledSinceLastMaintenance = document.Statistics?.DistanceTravelledSinceLastMaintenanceInMiles ?? 0 + travelledDistance;
                maintenanceRequired = distanceTravelledSinceLastMaintenance > 50000;
            }

            document.Statistics.DistanceTravelledSinceLastMaintenanceInMiles = distanceTravelledSinceLastMaintenance;
            document.Statistics.MaintenanceRequired = maintenanceRequired;

            document.Statistics.TotalDistanceTravelledInMiles = document.Statistics.TotalDistanceTravelledInMiles + travelledDistance;
            document.Statistics.AirtimeInMinutes = document.Statistics.AirtimeInMinutes + timeTaken;

        }
    }
}