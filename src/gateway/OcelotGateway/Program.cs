using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OcelotGateway.Health;
using OcelotGateway.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ── JWT validation at the gateway level ──
// Ocelot delegates auth to this scheme when a route has AuthenticationOptions set.
var jwt = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwt["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer            = true,
            ValidateAudience          = true,
            ValidateLifetime          = true,
            ValidateIssuerSigningKey  = true,
            ValidIssuer               = jwt["Issuer"],
            ValidAudience             = jwt["Audience"],
            IssuerSigningKey          = new SymmetricSecurityKey(key),
            ClockSkew                 = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Gateway self health check
builder.Services.AddHealthChecks();

// HttpClient used by the health aggregator — short timeout, no retries
builder.Services.AddHttpClient("health", client =>
{
    client.Timeout = TimeSpan.FromSeconds(6);
});

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();
app.UseCors("AllowAngular");

// Auth middleware must run before Ocelot so the JWT is validated before routing
app.UseAuthentication();
app.UseAuthorization();

// Gateway self health — GET /health (anonymous — monitoring tools need this)
app.MapHealthChecks("/health").AllowAnonymous();

// Aggregated health across all 5 services — GET /health/all
HealthAggregator.MapHealthAll(app);

await app.UseOcelot();
app.Run();
