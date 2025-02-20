using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;

namespace AuthApi.Controllers
{
    
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UbicanosController( ILogger<UbicanosController> logger, UbicanosService ubicanosService) : ControllerBase
    {

        private readonly ILogger<UbicanosController> logger = logger;
        private readonly UbicanosService ubicanosService = ubicanosService;


        /// <summary>
        /// Get the municipalities catalog
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Return the contact infor</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route("municipalities")]
        public async Task<IActionResult> GetMunicipalities()
        {
            try
            {
                var data = await ubicanosService.GetLocations();
                return Ok(data);
            }
            catch (System.Exception ex)
            {
                return BadRequest( new {
                    Title = "Error al obtener los datos",
                    Message = ex.Message
                });
            }
        }


        /// <summary>
        /// Get the municipality data
        /// </summary>
        /// <param name="municipalityId"></param>
        /// <returns></returns>
        /// <response code="200">Return the contact infor</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route("municipalities/{municipalityId}")]
        public async Task<IActionResult> GetMunicipalities([FromRoute] string municipalityId)
        {
            try
            {
                var data = await ubicanosService.GetMunicipality(municipalityId);
                return Ok(data);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound( new {
                    Title = "El municipio no se encuentra registrado",
                    Message = knf.Message
                } );
            }
            catch (System.Exception ex)
            {
                return BadRequest( new {
                    Title = "Error al obtener los datos",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get the municipality data
        /// </summary>
        /// <param name="municipalityId"></param>
        /// <returns></returns>
        /// <response code="200">Return the contact infor</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route("municipalities/{municipalityId}/locations")]
        public async Task<IActionResult> GetLocations([FromRoute] string municipalityId)
        {
            try
            {
                var data = await ubicanosService.GetLocations(municipalityId);
                return Ok(data);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound( new {
                    Title = "El municipio no se encuentra registrado",
                    Message = knf.Message
                } );
            }
            catch (System.Exception ex)
            {
                return BadRequest( new {
                    Title = "Error al obtener los datos",
                    Message = ex.Message
                });
            }
        }


    }
}
