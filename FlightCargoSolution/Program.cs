using FlightCargoSolution.ChangeStream;
using FlightCargoSolution.Interface;
using FlightCargoSolution.Services;
using MongoDB.Driver;

IConfiguration configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//Add Swagger
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMongoClient>(x => { return new MongoClient(configuration["MongoDbConnectionString"]); });
//builder.Services.AddSingleton<IConfiguration>(x => configuration);

//DI
builder.Services.AddSingleton<ICitiesInterface, CitiesService>();
builder.Services.AddSingleton<ICargoInterface, CargoService>();
builder.Services.AddSingleton<IPlanesInterface, PlaneService>();
ChangeStream.Monitor(new MongoClient(configuration["MongoDbConnectionString"])).ConfigureAwait(false);

var app = builder.Build();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
