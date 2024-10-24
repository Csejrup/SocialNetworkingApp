using Microsoft.EntityFrameworkCore;
using UserProfileService.Data;
using UserProfileService.Repositories;
using UserProfileService.Services;
using Shared.Messaging;
using EasyNetQ;

var builder = WebApplication.CreateBuilder(args);

// Database setup: Using in-memory for demo purposes
builder.Services.AddDbContext<UserProfileDbContext>(options =>
    options.UseInMemoryDatabase("UserProfilesDb"));

// Register repositories and services
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService.Services.UserProfileService>();

// Register RabbitMQ connection 
builder.Services.AddEasyNetQ("host=rabbitmq");

// Register MessageClient for handling RabbitMQ messaging
builder.Services.AddScoped<IMessageClient, MessageClient>();

// Register controllers, API docs, and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build the application
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();
    dbContext.Database.EnsureCreated();
}

// Development environment-specific configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Standard middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();