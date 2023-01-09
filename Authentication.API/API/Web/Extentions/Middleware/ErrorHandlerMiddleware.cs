namespace Web.Extentions.Middleware
{
    using System.Net;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Builder;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using Serilog;

    using Shared.Exceptions;

    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                await HandleExceptionAsync(context, error);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorResult = new ErrorResult
            {
                Exception = exception.Message.Trim(),
            };

            switch (exception)
            {


                case CustomException e:
                    errorResult.StatusCode = (int)e.StatusCode;
                    if (e.ErrorMessages is not null)
                    {
                        errorResult.Messages = e.ErrorMessages;
                    }

                    break;

                case KeyNotFoundException:
                    errorResult.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case NotImplementedException _:
                    errorResult.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorResult.Messages.Add("The requested operation is not implemented.");
                    break;

                case UnauthorizedAccessException _:
                    errorResult.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResult.Messages.Add("Access is denied.");
                    break;

                default:
                    errorResult.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResult.Messages.Add("An unexpected error occurred.");
                    break;
            }

            Log.Error($"{errorResult.Exception} Request failed with Status Code {errorResult.StatusCode}.");

            var response = context.Response;

            response.ContentType = "application/json";
            response.StatusCode = errorResult.StatusCode;

            await response.WriteAsync(SerializeObject(errorResult));
        }

        private static string SerializeObject(object obj)
            => JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(true, true)
                }
            });

        private class ErrorResult
        {
            public List<string> Messages { get; set; } = new();
            public string? Exception { get; set; }
            public int StatusCode { get; set; }
        }
    }

    public static class ValidationExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidationExceptionHandler(this IApplicationBuilder builder)
            => builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}