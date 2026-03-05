using AirportApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Tilføjer services til containeren.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<IMessageProducer, RabbitMQProducer>();

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
