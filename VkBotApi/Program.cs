using EsConsultationServices;
using EsConsultationServices.Contracts;
using EsConsultationServices.Films;
using EsConsultationServices.Workflow;
using EsDb;
using FilmDb;
using Microsoft.EntityFrameworkCore;
using VkBotApi.Bot;
using VkBotApi.Bot.Commands;
using VkBotApi.Options;
using VkBotApi.Parsing;
using VkBotApi.Queue;
using VkNet;
using VkNet.Model;

var builder = WebApplication.CreateBuilder(args);

var esConnectionString = builder.Configuration.GetConnectionString("EsConnection")
    ?? throw new InvalidOperationException("Connection string 'EsConnection' not found.");
var filmConnectionString = builder.Configuration.GetConnectionString("FilmConnection")
    ?? throw new InvalidOperationException("Connection string 'FilmConnection' not found.");

var vkOptions = builder.Configuration.GetSection(VkOptions.SectionName).Get<VkOptions>() ?? new VkOptions();
if (string.IsNullOrWhiteSpace(vkOptions.AccessToken))
    throw new InvalidOperationException("Не задан Vk:AccessToken.");
if (string.IsNullOrWhiteSpace(vkOptions.ConfirmationToken))
    throw new InvalidOperationException("Не задан Vk:ConfirmationToken.");

builder.Services.Configure<ConsultationOptions>(
    builder.Configuration.GetSection(ConsultationOptions.SectionName));
builder.Services.Configure<VkOptions>(
    builder.Configuration.GetSection(VkOptions.SectionName));

builder.Services.AddDbContext<EsDbContext>(options => options.UseSqlServer(esConnectionString));
builder.Services.AddDbContext<FilmDbContext>(options => options.UseSqlServer(filmConnectionString));

// Провайдер описаний советов читает FilmDb, поэтому регистрируется до AddEsConsultationServices.
builder.Services.AddScoped<IConsultationDisplayProvider, AdviceConsultationDisplayProvider>();
builder.Services.AddEsConsultationServices();

builder.Services.AddHttpClient();

builder.Services.AddSingleton(_ =>
{
    var api = new VkApi();
    api.Authorize(new ApiAuthParams { AccessToken = vkOptions.AccessToken });
    return api;
});

builder.Services.AddSingleton<VkUpdateQueue>();
builder.Services.AddHostedService<VkUpdateProcessor>();

builder.Services.AddScoped<VkCallbackParser>();
builder.Services.AddScoped<VkCommandParser>();
builder.Services.AddScoped<VkKeyboardFactory>();
builder.Services.AddScoped<FilmMessageFormatter>();
builder.Services.AddScoped<VkFilmPresenter>();
builder.Services.AddScoped<VkStepPresenter>();
builder.Services.AddScoped<VkBotHandler>();
builder.Services.AddScoped<IVkMessageSender, VkMessageSender>();

builder.Services.AddScoped<IVkCommand, StartCommand>();
builder.Services.AddScoped<IVkCommand, StartConsultationCommand>();
builder.Services.AddScoped<IVkCommand, SetAnswerAndNextCommand>();
builder.Services.AddScoped<IVkCommand, AnotherCommand>();
builder.Services.AddScoped<IVkCommand, ReviewCommand>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program
{
}
