

using FlightCargoSolution.Models;

namespace FlightCargoSolution.Interface
{
    public interface IPlanesInterface
    {
        Task<IEnumerable<Plane>> GetAllPlanes();
        Task<Plane> GetPlaneById(string Id);
        Task<Plane> MovePlaneLocation(string id, string location, int heading);
        Task<bool> AddDestination(string id, string city);
        Task<bool> UpdateDestination(string id, string city);
        Task<bool> RemoveDestination(string id);
        Task<Plane> UpdateLandPlaneLocation(string id, string location, int heading, string city);
        string GetLastError();
    }
}
