using Abstractions.Interfaces;
using BFB.BusinessServices;
using BFB.DataAccess.MSSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register MSSQL services with the environment information
builder.Services.AddMSSQL(builder.Configuration, builder.Environment);

// Register business services
builder.Services.AddBusinessServices();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BFB Bank Account API v1");
        // Set Swagger UI at the app's root
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
