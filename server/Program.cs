using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Utils;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequireNonAlphanumeric = false;
    opt.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<UserDataContext>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme).Configure(options =>
{
    // TODO: Remove as this is only for testing
    options.BearerTokenExpiration = TimeSpan.FromSeconds(10);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var autoUpdateDb = builder.Configuration.GetValue<bool>("AUTO_UPDATE_DB");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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