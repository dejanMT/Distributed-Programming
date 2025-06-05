using MongoDB.Driver;
using BookingService.Models;
using BookingService.Services;


var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSingleton<IMongoClient>(s =>
//    new MongoClient("mongodb://localhost:27017"));
//builder.Services.AddScoped<IMongoDatabase>(s =>
//    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
//builder.Services.AddScoped<IMongoCollection<Booking>>(s =>
//    s.GetRequiredService<IMongoDatabase>().GetCollection<Booking>("Bookings"));

builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetSection("MongoDB").Value));
builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoDatabase>().GetCollection<Booking>("Bookings"));

builder.Services.AddScoped<BookingService.Services.BookingService>();

// Add services to the container.

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

app.UseAuthorization();

app.MapControllers();

app.Run();
