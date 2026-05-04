using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace OcelotGateway.Health
{
    /// <summary>
    /// Fans out to all 5 downstream services in parallel, collects their
    /// /health responses, and returns a single aggregated status document.
    /// Exposed at GET /health/all — no auth required (monitoring tools need it).
    /// </summary>
    public static class HealthAggregator
    {
        private static readonly ServiceTarget[] Services =
        [
            new("identity",  "http://localhost:5010/health"),
            new("catalog",   "http://localhost:5020/health"),
            new("workflow",  "http://localhost:5030/health"),
            new("reporting", "http://localhost:5040/health"),
            new("search",    "http://localhost:5050/health"),
        ];

        public static void MapHealthAll(WebApplication app)
        {
            app.MapGet("/health/all", async (IHttpClientFactory factory) =>
            {
                var client = factory.CreateClient("health");
                var tasks = Services.Select(s => CheckService(client, s)).ToArray();
                var results = await Task.WhenAll(tasks);

                var overallHealthy = results.All(r => r.Healthy);
                var statusCode = overallHealthy ? 200 : 503;

                var response = new
                {
                    status    = overallHealthy ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    gateway   = "healthy",
                    services  = results.Select(r => new
                    {
                        r.Name,
                        status      = r.Healthy ? "healthy" : "unhealthy",
                        responseMs  = r.ResponseMs,
                        detail      = r.Detail
                    })
                };

                return Results.Json(response, statusCode: statusCode);
            })
            .AllowAnonymous()
            .WithName("HealthAll")
            .WithTags("Health");
        }

        private static async Task<ServiceHealth> CheckService(HttpClient client, ServiceTarget target)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await client.GetAsync(target.Url, cts.Token);
                sw.Stop();

                string detail = string.Empty;
                try
                {
                    var body = await response.Content.ReadAsStringAsync();
                    // Try to extract a "status" field if the service returns JSON
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("status", out var statusProp))
                        detail = statusProp.GetString() ?? string.Empty;
                }
                catch { /* plain-text /health response — ignore */ }

                return new ServiceHealth(target.Name, response.IsSuccessStatusCode, sw.ElapsedMilliseconds, detail);
            }
            catch (Exception ex)
            {
                sw.Stop();
                var reason = ex is TaskCanceledException ? "timeout" : "unreachable";
                return new ServiceHealth(target.Name, false, sw.ElapsedMilliseconds, reason);
            }
        }

        private record ServiceTarget(string Name, string Url);
        private record ServiceHealth(string Name, bool Healthy, long ResponseMs, string Detail);
    }
}
