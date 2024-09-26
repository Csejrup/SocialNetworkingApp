using Microsoft.EntityFrameworkCore;
using Monitoring;
using RabbitMQ.Client;
using UserProfileService.Data;
using UserProfileService.Repositories;
using UserProfileService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserProfileDbContext>(options =>
    options.UseInMemoryDatabase("UserProfilesDb"));

builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService.Services.UserProfileService>();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory() { HostName = "rabbitmq" };
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

builder.Services.AddSingleton<IMessageBusConsumer, MessageBusConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserProfileDbContext>();
    dbContext.Database.EnsureCreated(); // For in-memory or testing environments
}

//var messageBusConsumer = app.Services.GetService<IMessageBusConsumer>();
//messageBusConsumer?.StartConsuming();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();