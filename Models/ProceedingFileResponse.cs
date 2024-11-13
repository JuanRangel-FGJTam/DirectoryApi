using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;

namespace AuthApi.Models
{
    public class ProceedingFileResponse
    {
        public int Id {get;set;}
        public string? FileName {get;set;}
        public string? FilePath {get;set;}
        public string? FileType {get;set;}
        public string? FileUrl {get;set;}
        public long FileSize {get;set;}
        public DateTime CreatedAt {get;set;}
        public DateTime UpdatedAt {get;set;}
        public DateTime? DeletedAt {get;set;}

        public static ProceedingFileResponse FromEnity(ProceedingFile pf){
            var proceedingFileResponse = new ProceedingFileResponse();
            proceedingFileResponse.Id = pf.Id;
            proceedingFileResponse.FileName = pf.FileName;
            proceedingFileResponse.FilePath = pf.FilePath;
            proceedingFileResponse.FileType = pf.FileType;
            proceedingFileResponse.FileSize = pf.FileSize;
            proceedingFileResponse.CreatedAt = pf.CreatedAt;
            proceedingFileResponse.UpdatedAt = pf.UpdatedAt;
            proceedingFileResponse.FileUrl = null;
            proceedingFileResponse.DeletedAt = null;
            return proceedingFileResponse;
        }

    }
}