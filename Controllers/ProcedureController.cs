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
        /// <response code="404">The person is not found</response>
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

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
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
                DenunciaId = request.DenunciaId!.Trim(),
                Observations = request.Observations!.Trim(),
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
        /// <response code="404">The person is not found</response>
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

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == _personID).Any()){
                return NotFound( new {
                    Message = "The person is not found"
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

        /// <summary>
        ///  allows to update the procedding by the person_id and the folio
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/procedures
        ///     {
        ///         status: string [required, max:24],
        ///         observations: string|null [max:200],
        ///         denunciaId: string|null [max:100]
        ///     }
        ///
        /// </remarks>
        /// <param name="personId">person id</param>
        /// <param name="folio">folio </param>
        /// <param name="updateProceedingRequest"> request payload</param>
        /// <response code="201">Resource updated</response>
        /// <response code="400">The request is not valid (person id is not GUID, the payload is not 'appliaction/json' )</response>
        /// <response code="404">The person or the proceding its not found</response>
        /// <response code="409">Fail to update the proceding</response>
        [HttpPatch]
        [Route("/api/people/{personId}/procedures/{folio}")]
        public IActionResult UpdateProcedingById([FromRoute] Guid personId, [FromRoute] string folio, [FromBody] UpdateProceedingRequest updateProceedingRequest ) {

            // * retrive the procedding
            var procedding = this.dbContext.Proceeding.Where( p=> p.PersonId == personId && p.Folio == folio && folio != null ).FirstOrDefault();
            if( procedding == null){
                return NotFound(new {Message = "No se encontro registro que concuerde con el Id"});
            }

            // * validate the person
            if( !this.dbContext.People.Where(item => item.Id == personId).Any()){
                return NotFound( new {
                    Message = "The person is not found"
                });
            }

            // * validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // * attempting to update the resource
            try {
                UpdateProceding(procedding, updateProceedingRequest);
                return StatusCode(201, new {Message = $"Procedimiento persona:'{personId}' folio:'{folio}' actualizado"} );
            }catch(Exception ex){
                this._logger.LogError(ex, "Fail to attemting to update the proceding id {id}", procedding.Id);
                return Conflict(new {
                    Message = "Error no controlado al actualizar",
                    ErrorMessage = ex.Message
                });
            }

        }

        /// <summary>
        ///  allows to update the procedding by the id
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/people/{personId}/procedures
        ///     {
        ///         status: string [required, max:24],
        ///         observations: string|null [max:200],
        ///         denunciaId: string|null [max:100]
        ///     }
        ///
        /// <param name="procedingId">procedding id</param>
        /// <param name="updateProceedingRequest"> request payload</param>
        /// <response code="201">Resource udpated</response>
        /// <response code="400">The request is not valid (the payload is not 'appliaction/json', etc. )</response>
        /// <response code="404">The proceding element its not found</response>
        /// <response code="409">Fail to update the proceding</response>
        [HttpPatch]
        [Route("{procedingId:int}")]
        public IActionResult UpdateProcedingById([FromRoute] int procedingId, [FromBody] UpdateProceedingRequest updateProceedingRequest ) {

            // * retrive the procedding
            var procedding = this.dbContext.Proceeding.Where( p=> p.Id == procedingId).FirstOrDefault();
            if( procedding == null){
                return NotFound(new {Message = "No se encontro registro que concuerde con el Id"});
            }

            // * validate request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // * attempting to update the resource
            try {
                UpdateProceding(procedding, updateProceedingRequest);
                return StatusCode(201, new {Message = $"Procedimiento id '{procedingId}' actualizado"} );
            }catch(Exception ex){
                this._logger.LogError(ex, "Fail to attemting to update the proceding id {id}", procedingId);
                return Conflict(new {
                    Message = "Error no controlado al actualizar",
                    ErrorMessage = ex.Message
                });
            }

        }

        private void UpdateProceding(Proceeding proceeding, UpdateProceedingRequest request){
            
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

            // * update the procceding
            proceeding.Status = proceedingStatus;

            if( !string.IsNullOrEmpty(request.DenunciaId) ){
                proceeding.DenunciaId = request.DenunciaId.Trim();
            }

            if( !string.IsNullOrEmpty(request.Observations) ){
                proceeding.Observations = request.Observations.Trim();
            }

            dbContext.Proceeding.Update(proceeding);
            dbContext.SaveChanges();

        }
        
    }
}
