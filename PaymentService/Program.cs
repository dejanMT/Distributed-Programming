using MongoDB.Driver;
using PaymentService.Models;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

string apiKey = builder.Configuration["TaxiAPI:Key"];
string host = builder.Configuration["TaxiAPI:Host"];

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient("mongodb://localhost:27017"));
builder.Services.AddScoped(s => s.GetRequiredService<IMongoClient>().GetDatabase("CabBookingDB"));
builder.Services.AddScoped(s => s.GetRequiredService<IMongoDatabase>().GetCollection<Payment>("Payments"));

builder.Services.AddHttpClient<PaymentServices>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7183");
});


builder.Services.AddHttpClient<PaymentServices>();
//builder.Services.AddScoped<PaymentService.Services.PaymentServices>();

//builder.Services.AddScoped<PaymentServices>(s =>
//{
//    var collection = s.GetRequiredService<IMongoCollection<Payment>>();
//    var httpClient = new HttpClient();
//    return new PaymentServices(collection, httpClient, apiKey, host);
//});



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
