using System.Text;
using APIGateway;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;


var builder = WebApplication.CreateBuilder(args);


// Load settings
var config = builder.Configuration.GetSection("Settings").Get<Settings>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var jwtIssuer = config.JwtIssuer;
var jwtKey = config.JwtKey;

// Add authetincation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    }
);


// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json");


// Add Ocelot services
builder.Services.AddOcelot();

var app = builder.Build();

app.UseAuthentication(); 
app.UseAuthorization();

// Use Ocelot middleware
app.UseOcelot().Wait();

app.Run();