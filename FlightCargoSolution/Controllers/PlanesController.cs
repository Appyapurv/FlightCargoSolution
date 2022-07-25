using FlightCargoSolution.Interface;
using FlightCargoSolution.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightCargoSolution.FlightCargoSolution
{
    [Route("/")]
    [ApiController]
    public class PlanesController : ControllerBase
    {
        private readonly IPlanesInterface _IPlanesInterface;
        private readonly ICitiesInterface _ICitiesInterface;
        private readonly ILogger _logger;
        private string lastError = string.Empty;
        //
        public PlanesController(IPlanesInterface IPlanesInterface, ICitiesInterface ICitiesInterface, ILogger<PlaneService> logger)
        {
            _IPlanesInterface = IPlanesInterface;
            _ICitiesInterface = ICitiesInterface;
            _logger = logger;
        }

        /// <summary>
        /// Get ALL Planes
        /// </summary>
        /// <returns></returns>
        [HttpGet("planes")]
        public async Task<IActionResult> GetAllPlanes()
        {
            var planes = await _IPlanesInterface.GetAllPlanes();
            if (planes.ToList().Count > 0)
            {
                return new OkObjectResult(planes);
            }
            else
            {
                lastError = $"Failed to GetPlanes";
                _logger.LogWarning(lastError);
                return StatusCode(404);
            }
        }

        /// <summary>
        /// get Planes by
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("planes/{Id}")]
        public async Task<IActionResult> GetPlaneById(string Id)
        {
            var plane = await _IPlanesInterface.GetPlaneById(Id);
            return plane != null ? new OkObjectResult(plane) : StatusCode(404);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        [HttpPut("planes/{id}/location/{location}/{heading}")]
        public async Task<IActionResult> MovePlaneLocation(string id, string location, int heading)
        {
            var result = await _IPlanesInterface.MovePlaneLocation(id, location, heading);
            if (result == null)
            {
                lastError = $"Bad Request";
                _logger.LogError("Bad Request");
                return new BadRequestObjectResult(_IPlanesInterface.GetLastError());
            }
            return new JsonResult(result);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        /// <param name="heading"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        [HttpPut("planes/{id}/location/{location}/{heading}/{city}")]
        public async Task<IActionResult> UpdateLandPlaneLocation(string id, [FromRoute] string location, int heading, string city)
        {
            if (string.IsNullOrEmpty(location))
            {
                _logger.LogWarning("Location information is invalid");
                return new BadRequestObjectResult("Location information is invalid");
            }
            var locations = location.Split(',');
            if (locations.Count() != 2)
            {
                _logger.LogError("Location information is invalid");
                return new BadRequestObjectResult("Location information is invalid");
            }
            var cityObtained = await _ICitiesInterface.GetCityByName(city);
            if (cityObtained == null)
            {
                _logger.LogWarning("Found invalid city");
                return new BadRequestObjectResult("Found invalid city");
            }

            var result = await _IPlanesInterface.UpdateLandPlaneLocation(id, location, heading, city);
            if (result == null)
            {
                _logger.LogError("could not able to update");
                return new BadRequestObjectResult(_IPlanesInterface.GetLastError());
            }
            return new JsonResult(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        [HttpPut("planes/{id}/route/{city}")]
        public async Task<IActionResult> AddDestination(string id, string city)
        {
            var cityObtained = await _ICitiesInterface.GetCityByName(city);
            if (cityObtained == null)
            {
                _logger.LogError("Found invalid city");
                return new BadRequestObjectResult("Found invalid city");
            }

            var result = await _IPlanesInterface.AddDestination(id, city);
            if (!result)
            {
                _logger.LogError($"Bad request- {_ICitiesInterface.GetLastError() }");
                return new BadRequestObjectResult(_ICitiesInterface.GetLastError());
            }
            return new JsonResult(result);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        [HttpPost("planes/{id}/route/{city}")]
        public async Task<IActionResult> UpdateDestination(string id, string city)
        {
            var cityObtained = await _ICitiesInterface.GetCityByName(city);
            if (cityObtained == null)
            {
                return new BadRequestObjectResult("Found invalid city");
            }

            var result = await _IPlanesInterface.UpdateDestination(id, city);
            if (!result)
            {
                return new BadRequestObjectResult(_ICitiesInterface.GetLastError());
            }

            return new JsonResult(result);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("planes/{id}/route/destination")]
        public async Task<IActionResult> RemoveDestination(string id)
        {
            var result = await _IPlanesInterface.RemoveDestination(id);
            if (!result)
            {
                return new BadRequestObjectResult(_IPlanesInterface.GetLastError());
            }

            return new JsonResult(result);
        }

    }
}
