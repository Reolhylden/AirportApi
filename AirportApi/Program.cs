using AirportApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Tilføjer services til containeren.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Vi bruger Singleton, da RabbitMQProducer nu holder en aktiv forbindelse åben
builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();

var app = builder.Build();

// Konfigurere HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
