using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Data.Utils;
using AuthApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AuthApi.Services
{
    public class CatalogService(ILogger<CatalogService> logger, DirectoryDBContext directoryDBContext)
    {

        private readonly ILogger<CatalogService> logger = logger;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;


        /// <summary>
        /// Get the list of document types stored
        /// </summary>
        public IEnumerable<DocumentType> GetDocumentTypes(bool withTrash = false){
            return this.directoryDBContext.DocumentTypes
                .Where( item => withTrash ?true :item.DeletedAt == null )
                .ToList();
        }

    }
}