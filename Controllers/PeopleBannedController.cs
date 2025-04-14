using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Services;

namespace AuthApi.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/people-banned")]
    public class PeopleBannedController(ILogger<PeopleBannedController> logger, PersonService personService, PersonBanService personBanService) : ControllerBase
    {
        private readonly ILogger<PeopleBannedController> _logger = logger;
        private readonly PersonService personService = personService;
        private readonly PersonBanService personBanService = personBanService;

        /// <summary>
        /// Return the people banned
        /// </summary>
        /// <param name="orderBy"> propertie name used for ordering by default 'createdAt' posibles ["id", "name", "firstName", "curp", "email", "createdAt"] </param>
        /// <param name="ascending"></param>
        /// <param name="take"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <response code="200">Returns the person</response>
        [HttpGet]
        public ActionResult<IEnumerable<PersonResponse>?> GetPeople( [FromQuery] string orderBy = "createdAt", [FromQuery] bool ascending = false, [FromQuery] int take = 5, [FromQuery] int offset = 0 )
        {
            var peopleQuery = this.personService.GetPeople().Where(p => p.BannedAt != null).AsQueryable();

            // * ordering and retrive the data
            string ordering = ascending ? $"{orderBy} asc" : $"{orderBy} desc";
            var peopleData = peopleQuery
                .OrderBy(ordering)
                .Skip(offset)
                .Take(take)
                .Select(p => PersonResponse.FromEntity(p))
                .ToArray<PersonResponse>();

            // * Return response
            return Ok(peopleData);
        }

        /// <summary>
        /// Ban a person
        /// </summary>
        /// <param name="personId"></param>
        /// <response code="200">Successfully banned the person</response>
        /// <response code="400">The request is not valid</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="404">Person not found</response>
        [HttpPost("{personId}")]
        public IActionResult BanPerson([FromRoute] Guid personId)
        {
            // * attempt to ban the person
            var (Success, Message, Person) = personBanService.BanPerson(personId);
            if(Success)
            {
                return Ok(new {
                    Message,
                    Person
                });
            }

            return Person == null
                ? NotFound(new { Message })
                : BadRequest(new { Message });
        }

        /// <summary>
        /// Unbans a person by clearing the BannedAt timestamp.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to unban.</param>
        /// <response code="200">Successfully unbanned the person</response>
        /// <response code="400">The person is not currently banned</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        /// <response code="404">Person not found</response>
        [HttpPost("{personId}/unban")]
        public IActionResult UnbanPerson([FromRoute] Guid personId)
        {
            // * attempt to unband the person
            var (Success, Message, Person) = this.personBanService.UnbanPerson(personId);
            if(Success)
            {
                return Ok(new {
                    Message,
                    Person
                });
            }

            return Person == null
                ? NotFound(new { Message })
                : BadRequest(new { Message });
        }

    }
}