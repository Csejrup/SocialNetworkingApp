using Microsoft.EntityFrameworkCore;
using InteractionService.Data;
using InteractionService.Repositories;
using InteractionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<InteractionDbContext>(options =>
    options.UseInMemoryDatabase("InteractionsDb"));

builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IInteractionService, InteractionService.Services.InteractionService>();
builder.Services.AddHttpClient<IInteractionService, InteractionService.Services.InteractionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InteractionDbContext>();
    dbContext.Database.EnsureCreated(); // For in-memory or testing environments
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();