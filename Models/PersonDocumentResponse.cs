using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;

namespace AuthApi.Models
{
    public class PersonDocumentResponse
    {
        public int Id {get;set;}
        public Guid PersonId {get;set;}
        public string? FileName {get;set;}
        public string? FilePath {get;set;}
        public string? MimmeType {get;set;}
        public string? FileUrl {get;set;}
        public long FileSize {get;set;}
        public int DocumentTypeId {get;set;}
        public string? DocumentTypeName {get;set;}
        public DateTime CreatedAt {get;set;}
        public DateTime? DeletedAt {get;set;}

        public static PersonDocumentResponse FromEnity(PersonFile pf)
        {
            var response = new PersonDocumentResponse();
            response.Id = pf.Id;
            response.FileName = pf.FileName;
            response.FilePath = pf.FilePath;
            response.MimmeType = pf.MimmeType;
            response.FileSize = pf.FileSize;
            response.CreatedAt = pf.CreatedAt;
            response.DeletedAt = pf.DeletedAt;
            response.DocumentTypeId = pf.DocumentType!.Id;
            response.DocumentTypeName = pf.DocumentType?.Name;
            response.FileUrl = null;
            return response;
        }

    }
}