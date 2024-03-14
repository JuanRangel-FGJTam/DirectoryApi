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
        public ActionResult<IEnumerable<State>> GetStates()
        {
            return Ok( dbContext.States.Include( c => c.Country ).ToList() );
        }

        /// <summary>
        ///  Get municipalities catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("municipalities")]
        public ActionResult<IEnumerable<Municipality>> GetMunicipalities()
        {   
            return Ok( dbContext.Municipalities.Include( c => c.State ).ToArray() );
        }
        
        /// <summary>
        ///  Get colonies catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("colonies")]
        public ActionResult<IEnumerable<Colony>> GetColonies()
        {
            return Ok( dbContext.Colonies.Include( c =>c.Municipality ).ToArray() );
        }
        
    }
}