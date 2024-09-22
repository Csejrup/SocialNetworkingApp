using Microsoft.EntityFrameworkCore;
using TweetPostingService.Data;
using TweetPostingService.Repositories;
using TweetPostingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TweetDbContext>(options =>
    options.UseInMemoryDatabase("TweetsDb"));

// Register the repository and services
builder.Services.AddScoped<ITweetRepository, TweetRepository>();
builder.Services.AddScoped<ITweetService, TweetService>();

// Register the RabbitMQ publisher
builder.Services.AddSingleton<IMessageBusPublisher, MessageBusPublisher>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TweetDbContext>();
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