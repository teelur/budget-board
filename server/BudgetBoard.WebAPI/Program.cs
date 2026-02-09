using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Utils;
using BudgetBoard.WebAPI.Jobs;
using BudgetBoard.WebAPI.Services;
using BudgetBoard.WebAPI.Utils;
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
var clientAddress = builder.Configuration.GetValue<string>("CLIENT_ADDRESS");
if (string.IsNullOrEmpty(clientAddress))
{
    throw new ArgumentNullException(nameof(clientAddress));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(clientAddress);
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

builder.Services.AddDbContext<UserDataContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddLocalization();

var oidcEnabled = builder.Configuration.GetValue<bool>("OIDC_ENABLED");
var disableLocalAuth = builder.Configuration.GetValue<bool>("DISABLE_LOCAL_AUTH");

// Validate configuration: if local auth is disabled, OIDC must be enabled
if (disableLocalAuth && !oidcEnabled)
{
    throw new InvalidOperationException(
        "OIDC_ENABLED must be true when DISABLE_LOCAL_AUTH is true. "
            + "Cannot disable local authentication without an alternative authentication method."
    );
}

if (oidcEnabled)
{
    // Validate OIDC configuration is present for token service
    var oidcAuthority = builder.Configuration.GetValue<string>("OIDC_ISSUER");
    var oidcClientId = builder.Configuration.GetValue<string>("OIDC_CLIENT_ID");
    var oidcClientSecret = builder.Configuration.GetValue<string>("OIDC_CLIENT_SECRET");

    var missingConfigs = new List<string>();
    if (string.IsNullOrEmpty(oidcAuthority))
        missingConfigs.Add("OIDC_ISSUER");
    if (string.IsNullOrEmpty(oidcClientId))
        missingConfigs.Add("OIDC_CLIENT_ID");
    if (string.IsNullOrEmpty(oidcClientSecret))
        missingConfigs.Add("OIDC_CLIENT_SECRET");
    if (missingConfigs.Count > 0)
    {
        throw new InvalidOperationException(
            "The following OIDC configuration values are missing or empty: "
                + string.Join(", ", missingConfigs)
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
    .AddCookie(IdentityConstants.ApplicationScheme);
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
        opt.SignIn.RequireConfirmedEmail = !string.IsNullOrEmpty(emailSender) && !disableLocalAuth;
    })
    .AddEntityFrameworkStores<UserDataContext>()
    .AddApiEndpoints();

// Configure email sender only when local auth is enabled (for email confirmation)
if (!string.IsNullOrEmpty(emailSender) && !disableLocalAuth)
{
    builder.Services.AddTransient<IEmailSender, EmailSender>();
}

builder.Services.ConfigureOptions<ConfigureJsonOptions>();
builder.Services.AddControllers();

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
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<ISimpleFinService, SimpleFinService>();
builder.Services.AddScoped<ILunchFlowService, LunchFlowService>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IAutomaticRuleService, AutomaticRuleService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IValueService, ValueService>();
builder.Services.AddScoped<IWidgetSettingsService, WidgetSettingsService>();
builder.Services.AddScoped<INetWorthWidgetLineService, NetWorthWidgetLineService>();
builder.Services.AddScoped<INetWorthWidgetCategoryService, NetWorthWidgetCategoryService>();
builder.Services.AddScoped<INetWorthWidgetGroupService, NetWorthWidgetGroupService>();
builder.Services.AddScoped<ISimpleFinOrganizationService, SimpleFinOrganizationService>();
builder.Services.AddScoped<ISimpleFinAccountService, SimpleFinAccountService>();
builder.Services.AddScoped<
    IAutomaticTransactionCategorizerService,
    AutomaticTransactionCategorizerService
>();
builder.Services.AddScoped<ILunchFlowAccountService, LunchFlowAccountService>();

var app = builder.Build();

// Log authentication configuration
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var disableNewUsers = builder.Configuration.GetValue<bool>("DISABLE_NEW_USERS");

if (disableLocalAuth)
{
    logger.LogInformation("Local authentication disabled - OIDC only");
}
else if (oidcEnabled)
{
    logger.LogInformation("Both local and OIDC authentication enabled");
}
else
{
    logger.LogInformation("Local authentication only");
}

if (disableNewUsers)
{
    logger.LogInformation("New user creation disabled - existing users only");
}
else
{
    logger.LogInformation("New user creation enabled");
}

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
        ExcludeRegisterPost =
            builder.Configuration.GetValue<bool>("DISABLE_NEW_USERS") || disableLocalAuth,
        ExcludeLoginPost = disableLocalAuth,
        ExcludeLogoutPost = false, // Keep logout available even with OIDC
        ExcludeForgotPasswordPost = disableLocalAuth,
        ExcludeResetPasswordPost = disableLocalAuth,
        ExcludeResendConfirmationEmailPost = disableLocalAuth,
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

// Add localization support after authentication so user identity is available
var supportedCultures = SupportedLanguages.SupportedCultureNames.ToArray();
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// Insert custom user language provider at the beginning of the list
// Falls back to browser locale if user hasn't configured a language
localizationOptions.RequestCultureProviders.Insert(0, new UserLanguageCultureProvider());

app.UseRequestLocalization(localizationOptions);

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
