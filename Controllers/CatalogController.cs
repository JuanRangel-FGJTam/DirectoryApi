using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthApi.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController(ILogger<CatalogController> logger, AuthDbContext context) : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger = logger;
        private readonly AuthDbContext dbContext = context;

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

    }
}