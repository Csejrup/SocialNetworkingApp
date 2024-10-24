using Microsoft.EntityFrameworkCore;
using InteractionService.Data;
using InteractionService.Repositories;
using InteractionService.Services;
using Shared.Messaging; 
using EasyNetQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<InteractionDbContext>(options =>
    options.UseInMemoryDatabase("InteractionsDb"));

// Register repositories and services
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IInteractionService, InteractionService.Services.InteractionService>();

// Register EasyNetQ and MessageClient for RabbitMQ communication
builder.Services.AddEasyNetQ("host=rabbitmq");
builder.Services.AddScoped<IMessageClient, MessageClient>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InteractionDbContext>();
    dbContext.Database.EnsureCreated(); // For in-memory or testing environments
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();