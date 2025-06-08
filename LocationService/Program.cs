using LocationService.Models;
using MongoDB.Driver;
using LocationService.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(Int32.Parse(port));
});


//For deplyment 
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetSection("MongoDB").Value));

builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoDatabase>().GetCollection<Location>("Locations"));

builder.Services.AddScoped<LocationService.Services.LocationService>();
//builder.Services.AddSingleton<WeatherService>();
builder.Services.AddHttpClient<WeatherService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
