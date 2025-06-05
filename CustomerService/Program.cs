using Middleware;
using CustomerService.Data;
using CustomerService.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using CustomerService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;


var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(Int32.Parse(port));
});

//To test locally
//builder.Services.AddSingleton<IMongoClient>(s =>
//    new MongoClient("mongodb://localhost:27017"));
//builder.Services.AddScoped<IMongoDatabase>(s =>
//    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
//builder.Services.AddScoped<IMongoCollection<User>>(s =>
//    s.GetRequiredService<IMongoDatabase>().GetCollection<User>("Users"));

//To connect to MongoDB Atlas
//builder.Services.AddSingleton<IMongoClient>(s =>
//    new MongoClient(builder.Configuration.GetSection("MongoDB:ConnectionString").Value));

//For deplyment 
builder.Services.AddSingleton<IMongoClient>(s => 
    new MongoClient(builder.Configuration.GetSection("MongoDB").Value));

builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoDatabase>().GetCollection<User>("Users"));

builder.Services.AddTransient<IEncryptor, Encryptor>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped<IJwtBuilder, JwtBuilder>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

IdentityModelEventSource.ShowPII = true;


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
