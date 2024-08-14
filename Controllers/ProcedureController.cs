using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Services;
using Azure.Core;

namespace AuthApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProcedureController(ILogger<ProcedureController> logger, DirectoryDBContext context) : ControllerBase
    {
        #region Injected properties
        private readonly ILogger<ProcedureController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;
        #endregion


        /// <summary>
        /// almacena los datos de los trámites que realiza el ciudadano a través de las diversas aplicaciones.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/procedures
        ///     {
        ///         folio: string [required, max:100]
        ///         status: string|null [max:24],
        ///         area: string|null [max:100],
        ///         name: string [required, max:120],
        ///         observations: string|null [max:200],
        ///         denunciaId: string|null [max:100],
        ///         created_at: string|null [format('yyy-mm-dd HH:mm')]
        ///     }
        ///
        /// </remarks>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <param name="request"></param>
        /// <response code="201">Succsessfull create the new record</response>
        /// <response code="400">The request is not valid (person id is not GUID, the payload is not 'appliaction/json' )</response>
        /// <response code="422">Some request params are not valid</response>
        [HttpPost]
        [Route("/api/people/{personId}/procedures")]
        public IActionResult Store( [FromRoute] string personId, [FromBody] NewProceedingRequest request){
            
            // * Validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // * Validate person id
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personId );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id has formatted not valid"
                });
            }


            // * validate if the folio is aleady stored
            var exist = this.dbContext.Proceeding.Where( item => item.PersonId == _personID && item.Folio == request.Folio ).Any();
            if(exist){
                var errors = new List<object> {
                    new { Folio = "El folio ya se encuentra registrado a esta persona" }
                };
                return UnprocessableEntity(new {
                    Title = "Uno o mas campos tienen error",
                    Errors = errors
                });
            }


            // * find the status or create a new one
            ProceedingStatus? proceedingStatus = null;
            if( !string.IsNullOrEmpty(request.Status) ){
                proceedingStatus = dbContext.ProceedingStatus.Where(item => item.Name == request.Status).FirstOrDefault();
                if(proceedingStatus == null){
                    proceedingStatus = new ProceedingStatus {
                        Name = request.Status.Trim()
                    };
                    dbContext.ProceedingStatus.Add(proceedingStatus);
                    dbContext.SaveChanges();
                }
            }

            // * find the area or create a new one
            Area? area = null;
            if( !string.IsNullOrEmpty(request.Area) ){
                area = dbContext.Area.Where(item => item.Name == request.Area).FirstOrDefault();
                if(area == null){
                    area = new Area {
                        Name = request.Area.Trim()
                    };
                    dbContext.Area.Add(area);
                    dbContext.SaveChanges();
                }
            }

            // * parse datetime
            DateTime? datetime = DateTime.Now;
            if( request.CreatedAt != null){
                try {
                    datetime = DateTime.Parse(request.CreatedAt);
                }
                catch (System.Exception) { }
            }

            // * create the new resorce
            var newProceeding = new Proceeding {
                PersonId = _personID,
                Name = request.Name!.Trim(),
                Folio = request.Folio!.Trim(),
                Status = proceedingStatus,
                Area = area,
                DenunciaId = request.DenunciaId,
                Observations = request.Observations,
                CreatedAt = datetime??DateTime.Now
            };

            // * insert into db
            dbContext.Proceeding.Add( newProceeding );
            dbContext.SaveChanges();

            // Return response
            return Created("Proceeding stored", newProceeding);

        }


        /// <summary>
        /// retorna el listado de tramites almacenados de la persona
        /// </summary>
        /// <param name="personId"> identificador de la person in formato GUID</param>
        /// <response code="200">return the data</response>
        /// <response code="400">The request is not valid ore some error are present</response>
        [HttpGet]
        [Route("/api/people/{personId}/procedures")]
        public ActionResult<IEnumerable<ProceedingResponse>> GetPersonProcedings([FromRoute] string personId){
            
            // * Validate person id
            Guid _personID = Guid.Empty;
            try{
                _personID = Guid.Parse( personId );
            }catch(Exception){
                return BadRequest( new {
                    message = $"Person id has formatted not valid"
                });
            }

            
            // * get data
            var data = this.dbContext.Proceeding
                .Where(item=> item.PersonId == _personID)
                .Include(p => p.Status)
                .Include(p => p.Area)
                .ToList<Proceeding>();

            // * return the data
            return data.Select(item => ProceedingResponse.FromIdentity(item)).ToList<ProceedingResponse>();
        }
        
    }
}
