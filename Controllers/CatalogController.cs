#pragma warning disable CS8602 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;
using AuthApi.Models;
using AuthApi.Models.Responses;

namespace AuthApi.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController(ILogger<CatalogController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;

        private readonly int mexicoCountryId = 138;

        /// <summary>
        ///  Get ocupations catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("occupations")]
        public ActionResult<IEnumerable<Occupation>> GetOccupations()
        {
            return Ok( dbContext.Occupation.OrderBy( item => item.Name).ToArray());
        }

        /// <summary>
        ///  Get genders catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("genders")]
        public ActionResult<IEnumerable<Gender>> GetGenders()
        {
            return Ok( dbContext.Gender.OrderBy( item => item.Name).ToArray());
        }

        /// <summary>
        ///  Get nationalities catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("nationalities")]
        public ActionResult<IEnumerable<Nationality>> GetNationalities()
        {
            return Ok( dbContext.Nationality.OrderBy( item => item.Name).ToArray());
        }

        /// <summary>
        ///  Get marital status catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("marital-statuses")]
        public ActionResult<IEnumerable<MaritalStatus>> GetMaritalStatuses()
        {
            return Ok( dbContext.MaritalStatus.OrderBy(item => item.Name).ToArray() );
        }

        /// <summary>
        ///  Get contact types catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("contact-types")]
        public ActionResult<IEnumerable<ContactType>> GetContactTypes()
        {
            return Ok( dbContext.ContactTypes.OrderBy(item =>item.Name).ToArray() );
        }

        /// <summary>
        ///  Get countries catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("countries")]
        public ActionResult<IEnumerable<Country>> GetCountries()
        {
            return Ok( dbContext.Countries.OrderBy(item =>item.Name).ToArray() );
        }

        /// <summary>
        ///  Get states catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("states")]
        public ActionResult<IEnumerable<State>> GetStates([FromQuery] int country_id = 138)
        {
            return Ok( dbContext.States
                .Include( c => c.Country )
                .Where( item => item.Country!.Id == country_id)
                .OrderBy( item => item.Name)
                .ToList()
            );
        }

        /// <summary>
        ///  Store a new state
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("states")]
        public ActionResult<IEnumerable<State>> StoreStates(NewStateRequest request)
        {
            if(!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var country = dbContext.Countries.FirstOrDefault(item =>item.Id == request.CountryId);
            if(country == null)
            {
                var errors = new Dictionary<string,string>{{"countryId", "The country was not found"}};
                return UnprocessableEntity( new UnprocesableResponse(errors));
            }

            try
            {
                var newState = new State {
                    Country = country,
                    Name = request.Name!.Trim().ToUpper()
                };
                this.dbContext.States.Add( newState);
                this.dbContext.SaveChanges();
                return Ok();
            }
            catch (System.Exception err)
            {
                this._logger.LogError(err, "Fail at store the new state");
                return Conflict( new {
                    err.Message
                });
            }
        }

        /// <summary>
        ///  Get municipalities catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("municipalities")]
        public ActionResult<IEnumerable<Municipality>> GetMunicipalities([FromQuery] int state_id = 28)
        {   
            return Ok( dbContext.Municipalities
                .Include( c => c.State )
                .Where( c => c.State!.Id == state_id)
                .OrderBy( item => item.Name)
                .ToArray()
            );
        }

        /// <summary>
        ///  Store a new municipality
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("municipalities")]
        public ActionResult<IEnumerable<State>> StoreMunicipality(NewMunicipalityRequest request)
        {
            if(!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var country = dbContext.Countries.FirstOrDefault(item =>item.Id == request.CountryId);
            if(country == null)
            {
                var errors = new Dictionary<string,string>{{"countryId", "The country was not found"}};
                return UnprocessableEntity( new UnprocesableResponse(errors));
            }

            var state = dbContext.States.FirstOrDefault(item =>item.Id == request.StateId);
            if(state == null)
            {
                var errors = new Dictionary<string,string>{{"StateId", "The state was not found"}};
                return UnprocessableEntity( new UnprocesableResponse(errors));
            }

            try
            {
                var newModel = new Municipality {
                    State = state,
                    Name = request.Name!.Trim().ToUpper()
                };
                this.dbContext.Municipalities.Add(newModel);
                this.dbContext.SaveChanges();
                return Ok();
            }
            catch (System.Exception err)
            {
                this._logger.LogError(err, "Fail at store the new municipality");
                return Conflict( new {
                    err.Message
                });
            }
        }
        
        /// <summary>
        ///  Get colonies catalog
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("colonies")]
        public ActionResult<IEnumerable<Colony>> GetColonies( [FromQuery] int municipality_id = 41 )
        {
            return Ok( dbContext.Colonies
                .Include( c =>c.Municipality )
                .Where( item => item.Municipality!.Id == municipality_id)
                .OrderBy(item =>item.Name)
                .ToArray()
            );
        }

        /// <summary>
        ///  Store a new colony
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("colonies")]
        public ActionResult<IEnumerable<State>> StoreColony(NewColonyRequest request)
        {
            if(!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var country = dbContext.Countries.FirstOrDefault(item =>item.Id == request.CountryId);
            if(country == null)
            {
                var errors = new Dictionary<string,string>{{"countryId", "The country was not found"}};
                return UnprocessableEntity( new UnprocesableResponse(errors));
            }

            var state = dbContext.States.FirstOrDefault(item =>item.Id == request.StateId);
            if(state == null)
            {
                var errors = new Dictionary<string,string>{{"StateId", "The state was not found"}};
                return UnprocessableEntity( new UnprocesableResponse(errors));
            }

            var municipality = dbContext.Municipalities.FirstOrDefault(item =>item.Id == request.MunicipalityId);
            if(municipality == null)
            {
                var errors = new Dictionary<string,string>{{"MunicipalityId", "The municipality was not found"}};
                return UnprocessableEntity( new UnprocesableResponse(errors));
            }

            try
            {
                var newModel = new Colony {
                    Municipality = municipality,
                    Name = request.Name!.Trim().ToUpper(),
                    ZipCode = request.ZipCode ?? "0"
                };
                this.dbContext.Colonies.Add(newModel);
                this.dbContext.SaveChanges();
                return Ok();
            }
            catch (System.Exception err)
            {
                this._logger.LogError(err, "Fail at store the new colony");
                return Conflict( new {
                    err.Message
                });
            }
        }


        /// <summary>
        ///  Return the colonies by zipcode
        /// </summary>
        /// <param name="zipcode"></param>
        /// <param name="country_id"></param>
        /// <returns></returns>
        /// <response code="200">Return the colonies and citys</response>
        /// <response code="404">The zipcode was not found</response>
        /// <response code="406">The country selected has no data</response>
        /// <response code="401">Auth token is not valid or is not present</response>
        [HttpGet]
        [Route("/api/zipcode/{zipcode}")]
        public ActionResult<Object> GetColoniesByZipCode(string zipcode, [FromQuery] string? country_id)
        {

            if(!dbContext.Colonies.Where( item => item.ZipCode == zipcode).Any())
            {
                return NotFound( new {
                    Title = $"Zip code {zipcode} not found",
                    Message = $"Colonies with zip code {zipcode} not found",
                });
            }

            try
            {
                // * get the country
                int countryId = int.TryParse(country_id, out int _countryId) ? _countryId : mexicoCountryId;
                var currentCountry = dbContext.Countries.FirstOrDefault(item => item.Id == countryId);
                if(currentCountry == null)
                {
                    return NotFound( new {
                        Title = $"Coutnry '{country_id}' not found",
                        Message = $"The country id '{country_id}' is not found",
                    });
                }
                
                int[] statesId = dbContext.States
                    .Where(item => item.Country.Id == currentCountry.Id)
                    .Select(s => s.Id)
                    .ToArray();

                int[] municipalitiesId = dbContext.Municipalities
                    .Where(item => statesId.Contains(item.State.Id))
                    .Select( mun => mun.Id)
                    .ToArray();
                
                // * Validate if the country has data
                if( !statesId.Any() || !municipalitiesId.Any() )
                {
                    return StatusCode(406, new {
                        Message = "The Country selected has not states or municipalities"
                    });
                }

                // * get the colonies catalog
                var colonies = dbContext.Colonies
                    .Where( c => c.ZipCode == zipcode && municipalitiesId.Contains(c.Municipality.Id) )
                    .OrderBy( item => item.Name)
                    .Select( c => new {
                        c.Id,
                        c.Name,
                        c.ZipCode})
                    .ToArray();

                // * get the defatul colony
                var defaultColonyId = colonies.First().Id;
                var defaultColony = dbContext.Colonies
                    .Include(c => c.Municipality)
                        .ThenInclude(m => m.State)
                    .FirstOrDefault(c => c.Id == defaultColonyId && c.Municipality != null);

                var defaultMunicipalityId = defaultColony.Municipality?.Id ?? 0;
                var defaultStateId = defaultColony.Municipality?.State?.Id ?? 0;

                // * get the municipalities catalog
                var municipalities = dbContext.Municipalities
                    .Where(m => m.State.Id == defaultStateId)
                    .Select( m => new {
                        m.Id,
                        m.Name,
                        Default = m.Id == defaultMunicipalityId
                    })
                    .OrderBy( item => item.Name)
                    .ToArray();

                // * get the states catalog
                var states = dbContext.States
                    .Where( s => s.Country.Id == countryId)
                    .Select( s => new {
                        s.Id,
                        s.Name,
                        Default = s.Id == defaultStateId
                    })
                    .OrderBy( item => item.Name)
                    .ToArray();

                // * return the data
                return new {
                    Zipcode = zipcode,
                    colonies,
                    municipalities,
                    states,
                    Country = new {
                        currentCountry.Id,
                        currentCountry.Name,
                        currentCountry.ISO
                    }
                };
            }
            catch(Exception err)
            {
                this._logger.LogError(err, "Fail at get the colonies by zipcode: {message}", err.Message);
                return Conflict( new {
                    err.Message
                });
            }

        }

    }
}
#pragma warning restore CS8602