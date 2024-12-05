using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Utils;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var clientUrl = builder.Configuration.GetValue<string>("CLIENT_URL");
if (string.IsNullOrEmpty(clientUrl))
{
    throw new ArgumentNullException(nameof(clientUrl));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(clientUrl);
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowCredentials();
        });
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

var connectionString = new string("Host={HOST};Port=5432;Database={DATABASE};Username={USER};Password={PASSWORD}")
    .Replace("{HOST}", postgresHost)
    .Replace("{DATABASE}", postgresDatabase)
    .Replace("{USER}", postgresUser)
    .Replace("{PASSWORD}", postgresPassword);

System.Diagnostics.Debug.WriteLine("Connection string: " + connectionString);

builder.Services.AddDbContext<UserDataContext>(
    o => o.UseNpgsql(connectionString));

builder.Services.AddAuthorization();

// If the user sets the email env variables, then configure confirmation emails, otherwise disable.
var emailSender = builder.Configuration.GetValue<string>("EMAIL_SENDER");

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(opt =>
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
    .AddEntityFrameworkStores<UserDataContext>();

if (!string.IsNullOrEmpty(emailSender))
{
    builder.Services.AddTransient<IEmailSender, EmailSender>();
}

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme).Configure(options =>
{
    options.BearerTokenExpiration = TimeSpan.FromHours(1);
    options.RefreshTokenExpiration = TimeSpan.FromDays(14);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var autoUpdateDb = builder.Configuration.GetValue<bool>("AUTO_UPDATE_DB");

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
app.MyMapIdentityApi<ApplicationUser>();

// Activate the CORS policy
app.UseCors(MyAllowSpecificOrigins);

// Enable authentication and authorization after CORS Middleware
// processing (UseCors) in case the Authorization Middleware tries
// to initiate a challenge before the CORS Middleware has a chance
// to set the appropriate headers.
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/logout", async (SignInManager<ApplicationUser> signInManager,
    [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }
    return Results.Unauthorized();
}).RequireAuthorization(); // So that only authorized users can use this endpoint

app.MapControllers();

// Automatically apply any Db changes
if (autoUpdateDb)
{
    System.Diagnostics.Debug.WriteLine("Updating Db with latest migration...");
    using (var serviceScope = app.Services.CreateScope())
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<UserDataContext>();
        dbContext.Database.Migrate();
    }
}
else
{
    System.Diagnostics.Debug.WriteLine("Automatic Db updates not enabled.");
}

app.Run();
