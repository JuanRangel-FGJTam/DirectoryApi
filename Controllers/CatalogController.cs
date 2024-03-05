using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;

namespace AuthApi.Controllers
{
    [ApiController]
    [CAuthorize]
    [Route("api/[controller]")]
    public class CatalogController(ILogger<CatalogController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;

        [HttpGet]
        [Route("Occupations")]
        public ActionResult<IEnumerable<Occupation>> GetOccupations()
        {
            return Ok( dbContext.Occupation.ToArray() );
        }

        [HttpGet]
        [Route("Genders")]
        public ActionResult<IEnumerable<Gender>> GetGenders()
        {
            return Ok( dbContext.Gender.ToArray() );
        }

        [HttpGet]
        [Route("Nationalities")]
        public ActionResult<IEnumerable<Nationality>> GetNationalities()
        {
            return Ok( dbContext.Nationality.ToArray() );
        }

        [HttpGet]
        [Route("MaritalStatuses")]
        public ActionResult<IEnumerable<MaritalStatus>> GetMaritalStatuses()
        {
            return Ok( dbContext.MaritalStatus.ToArray() );
        }

        [HttpGet]
        [Route("ContactTypes")]
        public ActionResult<IEnumerable<ContactType>> GetContactTypes()
        {
            return Ok( dbContext.ContactTypes.ToArray() );
        }

        [HttpGet]
        [Route("Countries")]
        public ActionResult<IEnumerable<Country>> GetCountries()
        {
            return Ok( dbContext.Countries.ToArray() );
        }

        [HttpGet]
        [Route("States")]
        public ActionResult<IEnumerable<State>> GetStates()
        {
            return Ok( dbContext.States.Include( c => c.Country ).ToList() );
        }

        [HttpGet]
        [Route("Municipalities")]
        public ActionResult<IEnumerable<Municipality>> GetMunicipalities()
        {   
            return Ok( dbContext.Municipalities.Include( c => c.State ).ToArray() );
        }

        [HttpGet]
        [Route("Colonies")]
        public ActionResult<IEnumerable<Colony>> GetColonies()
        {
            return Ok( dbContext.Colonies.Include( c =>c.Municipality ).ToArray() );
        }
        
    }
}