using System;
using System.Net.Http.Headers;
using Serilog.Context;

namespace AuthApi.Data;

public class UserLoggingMiddleware (RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var values = new List<string?>();

        var userId = context.User?.FindFirst("userId")?.Value;
        if( userId == null)
        {
            values.Add("UnknownUser");
        }
        else
        {
            values.Add(userId);
            values.Add(context.User?.FindFirst("name")?.Value);
        }

        var userInfo = string.Join(";", values);

        using (LogContext.PushProperty("User", userInfo))
        {
            await _next(context);
        }
    }

}