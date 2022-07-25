using FlightCargoSolution.Models;

namespace FlightCargoSolution.Interface
{
    public interface ICargoInterface
    {

        Task<Cargo> CreateCargo(string location, string destination);
        Task<bool> CargoDelivered(string id);
        Task<Cargo> GetCargoById(string id);
        Task<bool> UpdateCargo(string id, string callsign);
        Task<Cargo> UnloadCargo(string id);

        Task<Cargo> UpdateCargoLocation(string id, string location);
        Task<List<Cargo>> GetCargoAtLocation(string location);
    }
}
