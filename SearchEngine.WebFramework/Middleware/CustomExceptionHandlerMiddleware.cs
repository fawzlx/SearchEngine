using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SearchEngine.Common;
using SearchEngine.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SearchEngine.WebFramework.Middleware;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

    public CustomExceptionHandlerMiddleware(RequestDelegate next,
        IWebHostEnvironment env,
        ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var httpStatusCode = HttpStatusCode.InternalServerError;
        var apiStatusCode = ApiResultStatusCode.ServerError;

        try
        {
            await _next(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest)
            {
                _logger.LogError("خطا در ثبت اطلاعات ورودی");
            }
        }
        catch (BadHttpRequestException exception)
        {
            _logger.LogError(exception, exception.Message);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogError(exception, exception.Message);
            await WriteToResponseAsync();
        }
        catch (SecurityTokenExpiredException exception)
        {
            _logger.LogWarning(exception, exception.Message);
            SetUnAuthorizeResponse(exception);
            await WriteToResponseAsync();
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, exception.Message);
            SetUnAuthorizeResponse(exception);
            await WriteToResponseAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);

            if (_env.IsDevelopment())
            {
                var dic = new Dictionary<string, string?>
                {
                    ["Exception"] = exception.Message,
                    ["StackTrace"] = exception.StackTrace,
                };
                JsonConvert.SerializeObject(dic);
            }
            await WriteToResponseAsync();
        }

        async Task WriteToResponseAsync()
        {
            if (context.Response.HasStarted)
                throw new InvalidOperationException("The response has already started, the http status code middleware will not be executed.");
            
            var json = JsonConvert.SerializeObject(new
            {
                IsSuccess = false,
                StatusCode = apiStatusCode,
                Message = apiStatusCode.ToDisplay()
            }, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            context.Response.StatusCode = (int)httpStatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }

        void SetUnAuthorizeResponse(Exception exception)
        {
            httpStatusCode = HttpStatusCode.Unauthorized;
            apiStatusCode = ApiResultStatusCode.UnAuthorized;

            if (!_env.IsDevelopment()) 
                return;

            var dic = new Dictionary<string, string?>
            {
                ["Exception"] = exception.Message,
                ["StackTrace"] = exception.StackTrace
            };
            if (exception is SecurityTokenExpiredException tokenException)
                dic.Add("Expires", tokenException.Expires.ToString(CultureInfo.InvariantCulture));

            JsonConvert.SerializeObject(dic);
        }
    }
}