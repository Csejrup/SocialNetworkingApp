using APIGateway;
using Monitoring;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

var app = builder.Build();

app.UseHttpsRedirection();


Thread.Sleep(2000);
Console.WriteLine("Starting simulator");
await ClientSimulator.Run();

// Use Ocelot as Middleware
await app.UseOcelot();



app.Run();