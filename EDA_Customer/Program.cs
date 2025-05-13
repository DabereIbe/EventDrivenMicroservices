using EDA_Customer.Data;
using EDA_Customer.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Shared.RabbitMQ;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<CustomerDbContext>(options => options.UseSqlite(@"Data Source = customer.Db"));

builder.Services.AddSingleton<IRabbitMqUtil, RabbitMqUtil>()
    .UseRabbitMqSettings()
    .AddHostedService<RabbitMqService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    db.Database.EnsureCreated();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
