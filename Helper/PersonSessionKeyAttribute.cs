using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Entities;
using Microsoft.Extensions.DependencyInjection;
using AuthApi.Services;

namespace AuthApi.Helper
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PersonSessionKeyAttribute : Attribute, IAuthorizationFilter
    {

        private static readonly string cookieName = "SessionToken";
        private static readonly string headerSessionName = "SessionToken";
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            
            var sessionService = context.HttpContext.RequestServices.GetService<SessionService>()!;

            // * Retrieve the sessionToken by the header, query param or cookie value
            string sessionToken = string.Empty;
            if(context.HttpContext.Request.Headers.ContainsKey(headerSessionName)){
                sessionToken = context.HttpContext.Request.Headers[headerSessionName]!;
            }else{
                string? t = context.HttpContext.Request.Query["t"];
                if( t != null){
                    sessionToken = t;
                }else{
                    sessionToken = context.HttpContext.Request.Cookies[ cookieName ] ?? "";
                }
            }
            
            if( string.IsNullOrEmpty(sessionToken)) { 
                context.Result = new JsonResult( new { 
                    Message = "Sesion token no encontrado."
                }){
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }


            // * Retrive ipaddress and userAgent
            string ipAddress = context.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For")
                ? context.HttpContext.Request.Headers["X-Forwarded-For"].ToString()
                : context.HttpContext.Connection.RemoteIpAddress!.ToString();
            string userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();


            // * Validate the token 
            var session = sessionService.ValidateSession( sessionToken, ipAddress, userAgent, out string message );
            if( session == null){
                context.Result = new JsonResult( new { 
                   message
                }){
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }else{
                context.HttpContext.Items["session_token"] = session!.Token;
                context.HttpContext.Items["session_person_id"] = session.Person.Id;
            }
        }
    }

}