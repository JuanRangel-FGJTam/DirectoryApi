using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Components.Web;

namespace AuthApi.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController(ILogger<CatalogController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;

        /// <summary>
        ///  Get ocupations catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("occupations")]
        public ActionResult<IEnumerable<Occupation>> GetOccupations()
        {
            return Ok( dbContext.Occupation.ToArray() );
        }

        /// <summary>
        ///  Get genders catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("genders")]
        public ActionResult<IEnumerable<Gender>> GetGenders()
        {
            return Ok( dbContext.Gender.ToArray() );
        }

        /// <summary>
        ///  Get nationalities catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("nationalities")]
        public ActionResult<IEnumerable<Nationality>> GetNationalities()
        {
            return Ok( dbContext.Nationality.ToArray() );
        }

        /// <summary>
        ///  Get marital status catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("marital-statuses")]
        public ActionResult<IEnumerable<MaritalStatus>> GetMaritalStatuses()
        {
            return Ok( dbContext.MaritalStatus.ToArray() );
        }

        /// <summary>
        ///  Get contact types catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("contact-types")]
        public ActionResult<IEnumerable<ContactType>> GetContactTypes()
        {
            return Ok( dbContext.ContactTypes.ToArray() );
        }

        /// <summary>
        ///  Get countries catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("countries")]
        public ActionResult<IEnumerable<Country>> GetCountries()
        {
            return Ok( dbContext.Countries.ToArray() );
        }

        /// <summary>
        ///  Get states catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("states")]
        public ActionResult<IEnumerable<State>> GetStates([FromQuery] int country_id)
        {
            return Ok( dbContext.States
                .Include( c => c.Country )
                .Where( item => item.Country!.Id == country_id)
                .ToList()
            );
        }

        /// <summary>
        ///  Get municipalities catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("municipalities")]
        public ActionResult<IEnumerable<Municipality>> GetMunicipalities([FromQuery] int state_id)
        {   
            return Ok( dbContext.Municipalities
                .Include( c => c.State )
                .Where( c => c.State!.Id == state_id)
                .ToArray()
            );
        }
        
        /// <summary>
        ///  Get colonies catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("colonies")]
        public ActionResult<IEnumerable<Colony>> GetColonies( [FromQuery] int municipality_id )
        {
            return Ok( dbContext.Colonies
                .Include( c =>c.Municipality )
                .Where( item => item.Municipality!.Id == municipality_id)
                .ToArray()
            );
        }



        /// <summary>
        ///  Return the colonies by zipcode
        /// </summary>
        /// <param name="zipcode"></param>
        /// <returns></returns>
        /// <response code="200">Return the colonies and citys</response>
        /// <response code="404">The zipcode was not found</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route("/api/zipcode/{zipcode}")]
        public ActionResult<Object> GetColoniesByZipCode(string zipcode)
        {

            if( !dbContext.Colonies.Where( item => item.ZipCode == zipcode).Any()){
                return NotFound( new {
                    Title = $"Zip code {zipcode} not found",
                    Message = $"Colonies with zip code {zipcode} not found",
                });
            }

            var colonies = dbContext.Colonies
            .Where( c => c.ZipCode == zipcode)
            .OrderBy( item => item.Name)
            .Select( c => new { 
                c.Id,
                c.Name,
                c.ZipCode})
            .ToArray();

            var defaultColony = dbContext.Colonies
                .Include( c => c.Municipality.State.Country)
                .Where( c => c.Id == colonies.First().Id)
                .First();

            var defaultCountry =  defaultColony.Municipality.State.Country;

            var states = dbContext.States
                .Where( s => s.Country.Id == defaultCountry.Id)
                .Select( s => new {
                    s.Id,
                    s.Name,
                    Default = s.Id == defaultColony.Municipality.State.Id ?1 :0
                })
                .OrderBy( item => item.Name)
                .ToArray();

            var municipalities = dbContext.Municipalities
                .Where( m => m.State.Id == defaultColony.Municipality.State.Id)
                .Select( m => new {
                    m.Id,
                    m.Name,
                    Default = m.Id == defaultColony.Municipality.Id ?1 :0
                })
                .OrderBy( item => item.Name)
                .ToArray();

            return new {
                Zipcode = zipcode,
                colonies,
                municipalities,
                states,
                Country = new {
                    defaultCountry.Id,
                    defaultCountry.Name,
                    defaultCountry.ISO
                }
            };
        }

    }
}