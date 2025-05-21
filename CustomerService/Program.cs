using Middleware;
using CustomerService.Data;
using CustomerService.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using CustomerService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient("mongodb://localhost:27017"));

builder.Services.AddScoped<IMongoDatabase>(s =>
    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));

builder.Services.AddScoped<IMongoCollection<User>>(s =>
    s.GetRequiredService<IMongoDatabase>().GetCollection<User>("Users"));

builder.Services.AddTransient<IEncryptor, Encryptor>();
builder.Services.AddScoped<UserService>();

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
