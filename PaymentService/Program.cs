using MongoDB.Driver;
using PaymentService.Models;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

string apiKey = builder.Configuration["TaxiAPI:Key"];
string host = builder.Configuration["TaxiAPI:Host"];

builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetSection("MongoDB").Value));

builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
builder.Services.AddScoped(s =>
    s.GetRequiredService<IMongoDatabase>().GetCollection<Payment>("Payments"));


builder.Services.AddHttpClient<PaymentServices>(client =>
{
    client.BaseAddress = new Uri("https://fare-service-521568789858.europe-west1.run.app");
});


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
