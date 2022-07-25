
using FlightCargoSolution.Interface;
using FlightCargoSolution.Models;
using FlightCargoSolution.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightCargoSolution.FlightCargoSolution.Controllers
{
    [Route("/")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ICitiesInterface _ICitiesInterface;
        private readonly ILogger _logger;
         
        public CitiesController(ICitiesInterface ICitiesInterface, ILogger<CitiesService> logger)
        {
            _ICitiesInterface = ICitiesInterface;
            _logger = logger;
        }

        /// <summary>
        /// Get Cities
        /// </summary>
        /// <returns>Get Cities</returns>
        [HttpGet("cities")]
        public async Task<List<City>> GetAllCities()
        {
            var cities = await _ICitiesInterface.GetAllCities();
            return cities.ToList();
        }

        /// <summary>
        /// GetCity
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Get City By Name

        [HttpGet("cities/{name}")]
        public async Task<dynamic> GetCityByName(string name)
        {
            var results = await _ICitiesInterface.GetCityByName(name);
            return new OkObjectResult(results);
        }

        /// <summary>
        /// GetNeighbouringCities
        /// </summary>
        /// <param name="name"></param>
        /// <param name="count"></param>
        /// <returns>Get Neighbouring Cities</returns>
        [HttpGet("cities/{name}/neighbors/{count}")]
        public async Task<IActionResult> GetNeighbouringCities(string name, long count)
        {
            var results = await _ICitiesInterface.GetNeighbouringCities(name, count);
            return new OkObjectResult(results);
        }

    }
}
