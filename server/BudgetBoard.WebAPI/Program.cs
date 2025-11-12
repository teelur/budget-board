using System.Text.Json.Serialization;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Jobs;
using BudgetBoard.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Setup CORS for the frontend
var clientUrl = builder.Configuration.GetValue<string>("CLIENT_URL");
if (string.IsNullOrEmpty(clientUrl))
{
    throw new ArgumentNullException(nameof(clientUrl));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(clientUrl);
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowCredentials();
        }
    );
});

// Setup the Db
var postgresHost = builder.Configuration.GetValue<string>("POSTGRES_HOST");
if (string.IsNullOrEmpty(postgresHost))
{
    throw new ArgumentNullException(nameof(postgresHost));
}
var postgresDatabase = builder.Configuration.GetValue<string>("POSTGRES_DATABASE");
if (string.IsNullOrEmpty(postgresDatabase))
{
    throw new ArgumentNullException(nameof(postgresDatabase));
}
var postgresUser = builder.Configuration.GetValue<string>("POSTGRES_USER");
if (string.IsNullOrEmpty(postgresUser))
{
    throw new ArgumentNullException(nameof(postgresUser));
}

var postgresPassword = builder.Configuration.GetValue<string>("POSTGRES_PASSWORD");

var postgresPort = builder.Configuration.GetValue<int?>("POSTGRES_PORT") ?? 5432;

var connectionString = new string(
    "Host={HOST};Port={PORT};Database={DATABASE};Username={USER};Password={PASSWORD}"
)
    .Replace("{HOST}", postgresHost)
    .Replace("{PORT}", postgresPort.ToString())
    .Replace("{DATABASE}", postgresDatabase)
    .Replace("{USER}", postgresUser)
    .Replace("{PASSWORD}", postgresPassword);

System.Diagnostics.Debug.WriteLine("Connection string: " + connectionString);

builder.Services.AddDbContext<UserDataContext>(o =>
    o.UseNpgsql(connectionString, op => op.MapEnum<Currency>("currency"))
);

var oidcEnabled = builder.Configuration.GetValue<bool>("OIDC_ENABLED");

if (oidcEnabled)
{
    // Validate OIDC configuration is present for token service
    var oidcAuthority = builder.Configuration.GetValue<string>("OIDC_ISSUER");
    var oidcClientId = builder.Configuration.GetValue<string>("OIDC_CLIENT_ID");
    var oidcClientSecret = builder.Configuration.GetValue<string>("OIDC_CLIENT_SECRET");

    if (string.IsNullOrEmpty(oidcAuthority))
    {
        throw new ArgumentNullException(
            "OIDC_ISSUER",
            "OIDC_ISSUER is required when OIDC_ENABLED is true"
        );
    }
    if (string.IsNullOrEmpty(oidcClientId))
    {
        throw new ArgumentNullException(
            "OIDC_CLIENT_ID",
            "OIDC_CLIENT_ID is required when OIDC_ENABLED is true"
        );
    }
    if (string.IsNullOrEmpty(oidcClientSecret))
    {
        throw new ArgumentNullException(
            "OIDC_CLIENT_SECRET",
            "OIDC_CLIENT_SECRET is required when OIDC_ENABLED is true"
        );
    }
}

// Configure Identity with cookie authentication
// Same configuration whether OIDC is enabled or not since frontend handles OIDC flow
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddCookie(IdentityConstants.ApplicationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

// If the user sets the email env variables, then configure confirmation emails, otherwise disable.
var emailSender = builder.Configuration.GetValue<string>("EMAIL_SENDER");

builder
    .Services.AddIdentityCore<ApplicationUser>(opt =>
    {
        opt.Password.RequiredLength = 3;
        opt.Password.RequiredUniqueChars = 0;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireDigit = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.User.RequireUniqueEmail = true;
        opt.SignIn.RequireConfirmedEmail = !string.IsNullOrEmpty(emailSender);
    })
    .AddEntityFrameworkStores<UserDataContext>()
    .AddApiEndpoints();

if (!string.IsNullOrEmpty(emailSender))
{
    builder.Services.AddTransient<IEmailSender, EmailSender>();
}

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new FlexibleStringConverter());
    });

//Add support to logging with SERILOG
builder.Host.UseSerilog(
    (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration)
);

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// This is needed for reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var autoUpdateDb = builder.Configuration.GetValue<bool>("AUTO_UPDATE_DB");

// Register the new provisioning service after Identity is configured
builder.Services.AddScoped<IExternalUserProvisioningService, ExternalUserProvisioningService>();

// Register OIDC token service if OIDC is enabled
if (oidcEnabled)
{
    builder.Services.AddScoped<IOidcTokenService, OidcTokenService>();
}

if (!builder.Configuration.GetValue<bool>("DISABLE_AUTO_SYNC"))
{
    var syncIntervalHours = builder.Configuration.GetValue<int?>("SYNC_INTERVAL_HOURS") ?? 8;
    if (syncIntervalHours < 1)
    {
        throw new ArgumentOutOfRangeException(
            "SYNC_INTERVAL_HOURS",
            "SYNC_INTERVAL_HOURS must be at least 1"
        );
    }

    builder.Services.AddQuartz(options =>
    {
        var jobKey = new JobKey("SyncBackgroundJob");
        options.AddJob<SyncBackgroundJob>(opts => opts.WithIdentity(jobKey));

        options.AddTrigger(trigger =>
            trigger
                .ForJob(jobKey)
                // Allow a minute for everything to settle after boot before starting the job
                .StartAt(DateBuilder.FutureDate(1, IntervalUnit.Minute))
                .WithSimpleSchedule(schedule =>
                    schedule.WithIntervalInHours(syncIntervalHours).RepeatForever()
                )
        );
    });

    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });
}

// Used to abstract datetime
builder.Services.AddSingleton<INowProvider, NowProvider>();

// Add the services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<ITransactionCategoryService, TransactionCategoryService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IInstitutionService, InstitutionService>();
builder.Services.AddScoped<ISimpleFinService, SimpleFinService>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IAutomaticRuleService, AutomaticRuleService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IValueService, ValueService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

//Add support to logging request with SERILOG
app.UseSerilogRequestLogging();

// Create routes for the identity endpoints
app.MyMapIdentityApi<ApplicationUser>(
    new BudgetBoard.Overrides.IdentityApiEndpointRouteBuilderOptions()
    {
        ExcludeRegisterPost = builder.Configuration.GetValue<bool>("DISABLE_NEW_USERS"),
    }
);

// Activate the CORS policy
app.UseCors(MyAllowSpecificOrigins);

// Enable authentication and authorization after CORS Middleware
// processing (UseCors) in case the Authorization Middleware tries
// to initiate a challenge before the CORS Middleware has a chance
// to set the appropriate headers.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Automatically apply any Db changes
if (autoUpdateDb)
{
    System.Diagnostics.Debug.WriteLine("Updating Db with latest migration...");
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<UserDataContext>();
    dbContext.Database.Migrate();
}
else
{
    System.Diagnostics.Debug.WriteLine("Automatic Db updates not enabled.");
}

app.Run();
