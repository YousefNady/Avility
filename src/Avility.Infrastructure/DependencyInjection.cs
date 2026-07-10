using System.Text;
using Avility.Application.Common.Interfaces;
using Avility.Infrastructure.Auth;
using Avility.Infrastructure.Identity;
using Avility.Infrastructure.Persistence;
using Avility.Infrastructure.Persistence.Interceptors;
using Avility.Infrastructure.Services;
using Avility.Infrastructure.Storage;
using Avility.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Avility.Infrastructure.Email;

namespace Avility.Infrastructure;

/// <summary>
/// Single entry point for wiring up everything this layer owns. Program.cs
/// calls only AddInfrastructure(configuration) - it doesn't know EF Core
/// or ASP.NET Identity are involved at all. This is the same principle as
/// IApplicationDbContext: consumers depend on a small, stable surface,
/// not on the implementation details behind it.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddSingleton<SmtpEmailSender>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IEmailSender, BackgroundEmailSender>();
        services.Configure<BackgroundJobsSettings>(configuration.GetSection(BackgroundJobsSettings.SectionName));
        services.AddHostedService<RefreshTokenCleanupService>();
        
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseSqlite(connectionString)
                .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        // Application depends on IApplicationDbContext, never on
        // ApplicationDbContext directly (see ADR 0001 and
        // IApplicationDbContext's own doc comment). This line is the only
        // place that connects the two.
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        AddIdentity(services);
        
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
 
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
 
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
 
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                options.Events = new JwtBearerEvents
                {
                    // Browsers can't attach an Authorization header to a
                    // WebSocket upgrade request, so SignalR's documented
                    // workaround is a query-string token. Accepted only
                    // for the hub path - every other endpoint still
                    // requires the header exactly as before.
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/messages"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    private static void AddIdentity(IServiceCollection services)
    {
        // AddIdentityCore, not the full AddIdentity<TUser, TRole>(). The
        // full AddIdentity() call is designed for server-rendered apps: it
        // registers a cookie authentication scheme, wires up
        // ExternalLogins, and assumes Identity is the only auth mechanism
        // in play. This is a JWT-only API - authentication middleware for
        // that is added in the next milestone via
        // AddAuthentication().AddJwtBearer(), as its own scheme.
        // AddIdentityCore gives exactly what's needed here: UserManager,
        // password hashing/validation, and (via the calls chained below)
        // RoleManager and SignInManager - without assuming or configuring
        // any particular authentication scheme.
        services.AddIdentityCore<ApplicationUser>(ConfigureIdentityOptions)
            .AddRoles<ApplicationRole>()
            // SignInManager wraps UserManager with lockout-aware credential
            // checking (CheckPasswordSignInAsync increments/reset the
            // failed-access counter automatically). Registering it now
            // means the JWT login handler in the next milestone can reuse
            // it directly instead of re-implementing lockout bookkeeping
            // by hand against UserManager.
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            // Required for any token-based Identity flow (password reset,
            // email confirmation, 2FA) even though none of those flows are
            // built yet - UserManager's Generate*TokenAsync methods throw
            // without a registered provider.
            .AddDefaultTokenProviders();
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        // Password policy. RequireNonAlphanumeric is off deliberately:
        // modern guidance (e.g. NIST 800-63B) favors password length over
        // forced symbol usage, which mostly just pushes people toward
        // predictable substitutions ("Password1!") without a real security
        // gain. RequiredUniqueChars stops trivial patterns like
        // "aaaaaaaA1" from satisfying the other rules.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 4;

        // User settings. Login is by email, so two accounts sharing an
        // email address would be a real bug, not just a UX nuisance.
        options.User.RequireUniqueEmail = true;

        // Sign-in options. Deliberately relaxed for MVP: there is no
        // email-sending capability built yet (no SMTP/email provider
        // configured anywhere in this solution), so requiring confirmation
        // before sign-in would lock every new user out permanently with no
        // way to confirm. Revisit and enable once an email service exists.
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Lockout policy. Five failed attempts before a 15-minute lockout
        // is a standard, unremarkable balance between brute-force
        // resistance and not permanently punishing a user who mistypes
        // their password a few times. AllowedForNewUsers = true means this
        // protection is active from an account's very first sign-in
        // attempt, not just after it "matures."
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    }
}
