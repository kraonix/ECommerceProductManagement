using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OcelotGateway.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logFilePath;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public LoggingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            // Navigate out to the main solution directory to store logs outside bin/obj
            _logFilePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "..", "logs", "gateway_logs.txt"));
            
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir!);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capture timing and request specifics before the pipeline modifies them
            var requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            var method = context.Request.Method;
            var path = context.Request.Path;
            var query = context.Request.QueryString;

            // Wait for Ocelot to process the request
            await _next(context);

            var statusCode = context.Response.StatusCode;

            // Extract email cleanly by inspecting the Authorization Header (if present) natively
            string userEmail = "Anonymous";
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(token))
                    {
                        var jwtToken = handler.ReadJwtToken(token);
                        // Extract standard ClaimTypes.Email mapped payload
                        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");
                        if (emailClaim != null)
                        {
                            userEmail = emailClaim.Value;
                        }
                    }
                }
                catch
                {
                    // Forged/corrupt token logs safely fall back to anonymous status
                }
            }

            // Strictly enforcing requested format design
            var logEntry = $@"------------------------------------------------
{userEmail} - {requestTime}
{method} {path}{query} completed with Status {statusCode}
-------------------------------------------------
";

            // Async Thread-Safe File Append
            await _lock.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_logFilePath, logEntry);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
