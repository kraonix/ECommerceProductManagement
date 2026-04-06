using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityService.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _logFilePath;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public LoggingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            // Aligning with globally structured logging outside bin/obj
            _logFilePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "..", "logs", "identity_logs.txt"));
            
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir!);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            var method = context.Request.Method;
            var path = context.Request.Path.ToString().ToLower();
            var query = context.Request.QueryString;

            string userEmail = "Anonymous";

            if (path.EndsWith("/api/auth/login") && method == "POST")
            {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, true, 1024, true))
                {
                    var bodyStr = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                    try
                    {
                        var doc = System.Text.Json.JsonDocument.Parse(bodyStr);
                        if (doc.RootElement.TryGetProperty("email", out var el))
                        {
                            userEmail = el.GetString() ?? "Anonymous";
                        }
                    }
                    catch { }
                }
            }

            await _next(context);

            var statusCode = context.Response.StatusCode;

            if (userEmail == "Anonymous")
            {
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
                            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");
                            if (emailClaim != null)
                            {
                                userEmail = emailClaim.Value;
                            }
                        }
                    }
                    catch { }
                }
            }

            string actionLog = $"{method} {context.Request.Path}{query} completed with Status {statusCode}";

            if (path.EndsWith("/api/auth/login"))
            {
                actionLog = statusCode == 200 ? "Login Execution - SUCCESS" : $"Login Attempt - FAILED (Status {statusCode})";
            }
            else if (path.EndsWith("/api/auth/revoke"))
            {
                actionLog = statusCode == 200 ? "Logout / Token Revoke - SUCCESS" : $"Logout Attempt - FAILED (Status {statusCode})";
            }
            else if (path.EndsWith("/api/auth/refresh-token"))
            {
                actionLog = statusCode == 200 ? "Token Refresh Cycle - SUCCESS" : $"Token Refresh Cycle - FAILED (Status {statusCode})";
            }

            var logEntry = $@"------------------------------------------------
{userEmail} - {requestTime}
{actionLog}
-------------------------------------------------
";

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
