using FlightCargoSolution.Interface;
using FlightCargoSolution.Models;
using FlightCargoSolution.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FlightCargoSolution.FlightCargoSolution.Controllers
{

    [Route("/")]
    [ApiController]
    public class CargoController : ControllerBase
    {
        private readonly ICargoInterface _ICargoInterface;
        private readonly ILogger _logger;

        public CargoController(ICargoInterface ICargoInterface, ILogger<PlaneService> logger)
        {
            _ICargoInterface = ICargoInterface;
            _logger = logger;
        }
        /// <summary>
        /// create cargo
        /// </summary>
        /// <param name="location"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost("cargo/{location}/to/{destination}")]
        public async Task<IActionResult> CreateCargo(string location, string destination)
        {
            var result = await _ICargoInterface.CreateCargo(location, destination);
            return new OkObjectResult(result);
        }
        /// <summary>
        /// CargoDelivered
        /// </summary>
        /// <param name="id"></param>
        /// <returns><Cargo Delivered/returns>
        [HttpPut("cargo/{id}/delivered")]
        public async Task<bool> CargoDelivered(string id)
        {
            var cargo = await _ICargoInterface.CargoDelivered(id);
            return cargo;
        }

        /// <summary>
        /// UpdateCargo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callsign"></param>
        /// <returns></returns>
        [HttpPut("cargo/{id}/courier/{callsign}")]
        public async Task<bool> UpdateCargo(string id, string callsign)
        {
            var result = await _ICargoInterface.UpdateCargo(id, callsign);
            return result;
        }
        /// <summary>
        /// CargoUnsetCourier
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Cargo Unset Courier</returns>
        [HttpDelete("cargo/{id}/courier")]
        public async Task<Cargo> CargoUnsetCourier(string id)
        {
            var result = await _ICargoInterface.UnloadCargo(id);
            return result;
        }
        /// <summary>
        /// CargoMove
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpPut("cargo/{id}/location/{location}")]
        public async Task<Cargo> CargoMove(string id, string location)
        {
            var result = await _ICargoInterface.UpdateCargoLocation(id, location);
            return result;
        }
        /// <summary>
        /// GetCargoAtLocation
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        [HttpGet("cargo/location/{location}")]
        public async Task<List<Cargo>> GetCargoAtLocation(string location)
        {
            var result = await _ICargoInterface.GetCargoAtLocation(location);
            return result;
        }
    }
}
