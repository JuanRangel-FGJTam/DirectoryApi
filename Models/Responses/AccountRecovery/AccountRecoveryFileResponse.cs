using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;

namespace AuthApi.Models
{
    public class AccountRecoveryFileResponse
    {
        public Guid Id {get;set;}
        public string? FileName {get;set;}
        public string? FilePath {get;set;}
        public string? FileType {get;set;}
        public string? FileUrl {get;set;}
        public long FileSize {get;set;}
        public DateTime CreatedAt {get;set;}
        public DateTime? DeletedAt {get;set;}

        public static AccountRecoveryFileResponse FromEntity(AccountRecoveryFile model){
            var resp = new AccountRecoveryFileResponse();
            resp.Id = model.Id;
            resp.FileName = model.FileName;
            resp.FilePath = model.FilePath;
            resp.FileType = model.FileType;
            resp.FileSize = model.FileSize;
            resp.CreatedAt = model.CreatedAt;
            resp.DeletedAt = model.DeletedAt;
            resp.FileUrl = null;
            return resp;
        }

    }
}