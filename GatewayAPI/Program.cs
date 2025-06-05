var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient("Customer", c =>
{
    c.BaseAddress = new Uri("https://customer-service-521568789858.europe-west1.run.app");
});

builder.Services.AddHttpClient("Payment", c =>
{
    c.BaseAddress = new Uri("https://payment-service-521568789858.europe-west1.run.app");
});

builder.Services.AddHttpClient("Location", c =>
{
    c.BaseAddress = new Uri("https://location-service-521568789858.europe-west1.run.app");
});

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
