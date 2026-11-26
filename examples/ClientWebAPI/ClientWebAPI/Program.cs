using DynamicQueryBuilder;
using DynamicQueryBuilder.Models.Enums.Helpers.Databases;
using DynamicQueryBuilder.Models.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DynamicQueryBuilder");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The connection string 'ConnectionStrings:DynamicQueryBuilder' is not set. Set it with " +
        "'dotnet user-secrets set \"ConnectionStrings:DynamicQueryBuilder\" \"<value>\"' " +
        "run in examples/ClientWebAPI/ClientWebAPI (user secrets are loaded only in the Development environment), " +
        "or with the environment variable 'ConnectionStrings__DynamicQueryBuilder'. " +
        "See examples/README.md for forms that keep the value out of shell history.");
}

var settings = new DynamicQueryBuilderSettings(connectionString, DatabaseDriver.POSTGRESQL);
builder.Services.AddDynamicQueryBuilder(settings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
