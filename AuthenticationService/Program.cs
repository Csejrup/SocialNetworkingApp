var builder = WebApplication.CreateBuilder(args);


// Load settings
var config = builder.Configuration.GetSection("Settings").Get<Settings>();

var jwtIssuer = config.JwtIssuer;
var jwtKey = config.JwtKey;

// Add services to the container.
builder.Services.AddSingleton(config);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // Ensure this line is present

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
