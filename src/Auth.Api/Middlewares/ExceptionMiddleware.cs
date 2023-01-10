using Auth.Domain.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace Auth.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiExceptionResponse error;

            switch (exception)
            {
                case UserExistException badRequestException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    error = new ApiExceptionResponse()
                    {
                        Reason = context.Response.StatusCode.ToString(),
                        Message = badRequestException.Message
                    };
                    break;

                case RoleNotExistException badRequestException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    error = new ApiExceptionResponse()
                    {
                        Reason = context.Response.StatusCode.ToString(),
                        Message = badRequestException.Message
                    };
                    break;

                case SuperAdminNotExistException badRequestException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    error = new ApiExceptionResponse()
                    {
                        Reason = context.Response.StatusCode.ToString(),
                        Message = badRequestException.Message
                    };
                    break;

                case UserNotFoundException responseException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    error = new ApiExceptionResponse()
                    {
                        Reason = context.Response.StatusCode.ToString(),
                        Message = responseException.Message
                    };
                    break;

                case GoogleAuthException badRequestException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    error = new ApiExceptionResponse()
                    {
                        Reason = context.Response.StatusCode.ToString(),
                        Message = badRequestException.Message
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    error = new ApiExceptionResponse()
                    {
                        Reason = "InternalServerError",
                        Message = "Internal server error occurred." + exception
                    };
                    break;
            }

            return context.Response.WriteAsync(JsonConvert.SerializeObject(error));
        }
    }
}