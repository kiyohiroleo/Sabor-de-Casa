using Microsoft.AspNetCore.Mvc;
using SaborDeCasa.Api.Data;
using SaborDeCasa.Api.Middlewares;
using SaborDeCasa.Api.Repositories;
using SaborDeCasa.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");


if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddScoped<IDbConnectionFactory, PostgresConnectionFactory>();
    builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
    builder.Services.AddScoped<IPratoRepository, PratoRepository>();
}
else
{
    builder.Services.AddSingleton<ICategoriaRepository, CategoriaEmMemoriaRepository>();
}

builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IPratoService, PratoService>();

var app = builder.Build();

app.UseMiddleware<TratamentoDeErrosMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sabor de Casa API v1");
});

app.UseCors("PermitirFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
