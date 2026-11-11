using EsConsultationApi.Infrastructure;
using EsConsultationApi.Options;
using EsConsultationApi.Services;
using EsConsultationServices;
using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsDb;
using FilmDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var esConnectionString = builder.Configuration.GetConnectionString("EsConnection")
    ?? throw new InvalidOperationException("Connection string 'EsConnection' not found.");
var filmConnectionString = builder.Configuration.GetConnectionString("FilmConnection")
    ?? throw new InvalidOperationException("Connection string 'FilmConnection' not found.");

builder.Services.Configure<ConsultationOptions>(
    builder.Configuration.GetSection(ConsultationOptions.SectionName));

builder.Services.AddDbContext<EsDbContext>(options => options.UseSqlServer(esConnectionString));
builder.Services.AddDbContext<FilmDbContext>(options => options.UseSqlServer(filmConnectionString));

// Провайдер описаний советов читает FilmDb, поэтому регистрируется до AddEsConsultationServices.
builder.Services.AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>();
builder.Services.AddEsConsultationServices();

builder.Services.AddScoped<ConsultationExpertSystemResolver>();
builder.Services.AddScoped<ConsultationShownFilmStore>();
builder.Services.AddScoped<ConsultationReviewStore>();
builder.Services.AddScoped<ConsultationWorkflow>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EsConsultationApi",
        Version = "v1",
        Description = "HTTP API консультаций экспертной системы «Фильмы»."
    });
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program
{
}
