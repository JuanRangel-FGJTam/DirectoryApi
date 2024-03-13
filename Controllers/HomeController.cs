using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Helper;
using AuthApi.Entities;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AuthApi.Controllers
{
    
    /// <summary> </summary>
    [Route("home")]
    public class HomeController(ILogger<HomeController> logger ) : ControllerBase
    {
        private readonly ILogger<HomeController> _logger = logger;

        /// <summary>
        /// Default endpoint
        /// </summary>
        /// <returns></returns>
        [HttpGet("/")]
        public IActionResult Index()
        {
            return Ok( new {
                title = "FGJTAM Directory API"
            });
        }

    }
}