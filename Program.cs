// Trustwin.FileService
// 
// This is a simplified foundation service to enable a quick and easy
// method for pushing files from an external source into an https rest
// service that can then be processed by the server platform.
// 
// upon upload the server can call an external process with the uploaded
// file as a parameter, and can be configured to call different processes
// based upon the file extension.

using Druware.Server;
using Druware.Server.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Druware.Server.Entities;
using Microsoft.AspNetCore.HttpOverrides;
using Trustwin.FileService;

// set up our default/testing constants, though most of these will be
// overridden by configuration options.

// a default connectionString for the use of the entity framework
const string connectionString = "Host=localhost;Database=druware;Username=postgres;Password=blahblahblah!";

// read the configuration files to set up the environment
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = new AppSettings(configuration);
var cs = (string.IsNullOrEmpty(settings.ConnectionString)) ? 
    connectionString : settings.ConnectionString;

// now set up the application and prepare it for startup
var builder = WebApplication.CreateBuilder(args);

// Create all of the DbContext foundations for whichever DatabaseType is defined
// in the configuration, and settings.

switch (settings.DbType)
{
    case DbContextType.Microsoft:
        builder.Services.AddDbContext<ServerContext>(
            conf => conf.UseSqlServer(cs));
        builder.Services.AddDbContext<ContentContext>(
            conf => conf.UseSqlServer(cs));
        
        builder.Services.AddDbContext<ServerContextMicrosoft>(
            conf => conf.UseSqlServer(cs));
        builder.Services.AddDbContext<ContentContextMicrosoft>(
            conf => conf.UseSqlServer(cs));
        break;
    case DbContextType.PostgreSql:
        builder.Services.AddDbContext<ServerContext>(
            conf => conf.UseNpgsql(cs));
        builder.Services.AddDbContext<ContentContext>(
            conf => conf.UseNpgsql(cs));
        
        builder.Services.AddDbContext<ServerContextPostgreSql>(
            conf => conf.UseNpgsql(cs));
        builder.Services.AddDbContext<ContentContextPostgreSql>(
            conf => conf.UseNpgsql(cs));
        break;
    default:
        throw new ArgumentOutOfRangeException();
}

// Set up the Identity Services
builder.Services.AddIdentity<User, Role>(config =>
    {
        config.Password.RequiredLength = 8;
        config.User.RequireUniqueEmail = true;
        config.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ServerContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "Trustwin.Services";
    config.LoginPath = "/login/";
});

// Will probably want AutoMapper ( NuGet: AutoMapper )
builder.Services.AddAutoMapper(typeof(Program));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run the migrations this API calls
app.MigrateDatabase();

app.Run();
