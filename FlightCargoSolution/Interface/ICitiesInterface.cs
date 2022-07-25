using FlightCargoSolution.Models;

namespace FlightCargoSolution.Interface
{
    public interface ICitiesInterface
    {
        Task<IEnumerable<City>> GetAllCities();
        Task<City> GetCityByName(string cityName);
        Task<dynamic> GetNeighbouringCities(string cityName, long count);
        string GetLastError();
    }
}
