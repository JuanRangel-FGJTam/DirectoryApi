using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class AccountRecoveryFileRequest
    {
        public IFormFile? File { get; set; }
        public int? DocumentTypeId { get; set; }
    }
}