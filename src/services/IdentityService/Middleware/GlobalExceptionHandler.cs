using IdentityService.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Title = "An error occurred while processing your request.",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case InvalidCredentialsException or TokenValidationException:
                    problemDetails.Status = StatusCodes.Status401Unauthorized;
                    break;
                case UserNotFoundException:
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    break;
                case EmailAlreadyRegisteredException or InvalidOperationException: // Keep backward compat
                    problemDetails.Status = StatusCodes.Status409Conflict;
                    break;
                case ArgumentException:
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    break;
                default:
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Server Error";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
