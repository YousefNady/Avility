using Asp.Versioning;
using Avility.API.Middleware;
using Avility.API.Hubs;
using Avility.API.HealthChecks;
using Avility.API.Common.Responses;
using Avility.Application;
using Avility.Application.Messages;
using Avility.Infrastructure;
using Avility.Infrastructure.Persistence;
using Avility.Infrastructure.Persistence.Seed;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

// Add services to the container.

builder.Services.AddResponseCompression(options =>
{
    // Safe here specifically because no response reflects a secret
    // alongside attacker-controlled input (see BREACH attack) - this API
    // only returns JWTs from login/register/refresh, never mixed with
    // arbitrary user-supplied content in the same payload.
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // [ApiController]'s default invalid-ModelState response bypasses
        // ApiResponse<T> entirely (malformed JSON, bad route/query
        // binding) - this runs before the controller action, so
        // GlobalExceptionHandler and FluentValidation never see it.
        // Unify it with every other validation failure in the API.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(
                ApiResponse<object>.FailureResponse("One or more validation errors occurred.", errors));
        };
    });


builder.Services.AddDataProtection();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddSignalR();
builder.Services.AddScoped<IMessageNotifier, SignalRMessageNotifier>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            // SignalR's browser client sends withCredentials: true by
            // default on its negotiate/connection requests, which
            // requires AllowCredentials() here - and browsers reject
            // AllowAnyOrigin() combined with AllowCredentials(), which is
            // exactly why AllowedOrigins has to be an explicit list.
            .AllowCredentials();
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Avility API",
        Version = "v1",
        Description = "Backend API for Avility, an inclusive recruitment platform connecting job seekers with disabilities to accessibility-committed employers.",
        Contact = new OpenApiContact
        {
            Name = "Source on GitHub",
            Url = new Uri("https://github.com/YousefNady/Avility")
        }
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT access token from /api/v1/auth/login, without the 'Bearer ' prefix."
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready" })
    .AddCheck<FileStorageHealthCheck>("file-storage", tags: new[] { "ready" });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();

builder.Services.AddRateLimiter(options =>
{
    var permitLimit = builder.Environment.IsEnvironment("Testing") ? 1000 : 5;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = permitLimit;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // Baseline protection for every endpoint, not just /auth - applies in
    // addition to any named policy, so "auth" still enforces its own
    // stricter limit on top of this one. Partitioned by authenticated
    // user ID when available, falling back to remote IP for anonymous
    // callers, so one user/IP can't consume everyone else's share.
    // Generous in Testing deliberately: a tight limit here would apply
    // to every integration test host and start rejecting the existing
    // test suite's normal multi-request flows, not just abusive traffic.
    var globalPermitLimit = builder.Environment.IsEnvironment("Testing") ? 100_000 : 200;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "anonymous"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = globalPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    await AdminSeeder.SeedAsync(scope.ServiceProvider);
}

if (allowedOrigins.Length == 0 && !app.Environment.IsDevelopment())
{
    app.Logger.LogWarning(
        "Cors:AllowedOrigins is empty - no browser-based frontend will be able to call this API in {Environment}.",
        app.Environment.EnvironmentName);
}

app.UseResponseCompression();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
var enableSwagger = builder.Configuration.GetValue<bool>("SwaggerSettings:EnableSwagger", false);
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// app.UseHttpsRedirection();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<MessagesHub>("/hubs/messages").RequireCors("Frontend");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // No dependency checks - only confirms the process is up and able to
    // route a request. An orchestrator restarts the container on
    // liveness failure, so this must never depend on the database.
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Only runs checks tagged "ready" - can this instance actually serve
    // traffic right now? A load balancer stops routing to an instance
    // that fails readiness, without restarting it (unlike liveness).
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
});

app.Run();

public partial class Program;
