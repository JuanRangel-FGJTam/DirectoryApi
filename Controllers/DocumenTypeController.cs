using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Validators.Address;
using AuthApi.Models.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthApi.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class DocumenTypeController(ILogger<DocumenTypeController> logger, DirectoryDBContext context) : ControllerBase
    {
        private readonly ILogger<DocumenTypeController> _logger = logger;
        private readonly DirectoryDBContext dbContext = context;

        /// <summary>
        ///  Get all document types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult<IEnumerable<DocumentType>> GetAllDocumentTypes()
        {
            return this.dbContext.DocumentTypes
                .Where( item => item.DeletedAt == null)
                .OrderBy( item => item.Name)
                .ToArray();
        }

        /// <summary>
        ///  Add a new document type
        /// </summary>
        /// <returns></returns>
        /// <response code="201">The parameters are not valid or missing</response>
        /// <response code="422">The parameters are not valid or missing</response>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(UnprocesableResponse), StatusCodes.Status422UnprocessableEntity)]
        public ActionResult<DocumentType> AddDocumentType([FromBody] NewDocumentTypeRequest request)
        {
            // * validate the request
            var validationResults = new NewDocumentTypeValidator().Validate(request);
            if(!validationResults.IsValid){
                return UnprocessableEntity( new UnprocesableResponse(validationResults.Errors));
            }

            // * create ther new entity
            var newDocumentType = new DocumentType{
                Name = request.Name!,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            this.dbContext.DocumentTypes.Add(newDocumentType);
            this.dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created, newDocumentType);
        }

        /// <summary>
        ///  Update a document type
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">Document updated</response>
        /// <response code="404">Document type not found</response>
        /// <response code="422">The parameters are not valid or missing</response>
        [HttpPatch("{documentTypeID}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdateDocumentType([FromRoute] int documentTypeID, [FromBody] NewDocumentTypeRequest request)
        {
            // * attempt to get the document type
            var documentTypeTarget = this.dbContext.DocumentTypes.FirstOrDefault(item => item.Id == documentTypeID && item.DeletedAt == null);
            if( documentTypeTarget == null){
                return NotFound( new {
                    Title = "No se encontro el tipo de documento",
                    Message = $"No se encontro el tipo de documento con id '{documentTypeID}'"
                });
            }

            // * validate the request
            var validationResults = new NewDocumentTypeValidator().Validate(request);
            if(!validationResults.IsValid){
                return UnprocessableEntity( new UnprocesableResponse(validationResults.Errors));
            }

            documentTypeTarget.Name = request.Name!;
            documentTypeTarget.UpdatedAt = DateTime.Now;
            this.dbContext.DocumentTypes.Update(documentTypeTarget);
            this.dbContext.SaveChanges();
            return Ok( new {
                Title = "Tipo de documento actualizado",
                Message = $"Tipo de document con id '{documentTypeID}', actualizado"
            });
        }

        /// <summary>
        ///  Delete the document type
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <returns></returns>
        [HttpDelete("{documentTypeID}")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DeleteDocumentType([FromRoute] int documentTypeID)
        {
            // * attempt to get the document type
            var documentTypeTarget = this.dbContext.DocumentTypes.FirstOrDefault(item => item.Id == documentTypeID && item.DeletedAt == null);
            if( documentTypeTarget == null){
                return NotFound( new {
                    Title = "No se encontro el tipo de documento",
                    Message = $"No se encontro el tipo de documento con id '{documentTypeID}'"
                });
            }

            documentTypeTarget.DeletedAt = DateTime.Now;
            this.dbContext.DocumentTypes.Update(documentTypeTarget);
            this.dbContext.SaveChanges();
            return Ok( new {
                Title = "Tipo de documento eliminado",
                Message = $"Tipo de document con id '{documentTypeID}' eliminado"
            });
        }

    }
}