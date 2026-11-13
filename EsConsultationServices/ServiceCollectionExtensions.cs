using EsConsultationServices.Contracts;
using EsConsultationServices.Engine;
using EsConsultationServices.Films;
using EsConsultationServices.Persistence;
using EsConsultationServices.Services;
using EsConsultationServices.Workflow;
using EsDb;
using EsDb.Repositories;
using FilmDb.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EsConsultationServices
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует движок и сервис консультации. Требует зарегистрированного EsDbContext.
        /// Провайдер отображения можно зарегистрировать до вызова, чтобы заменить провайдер по умолчанию.
        /// </summary>
        public static IServiceCollection AddEsConsultationServices(this IServiceCollection services)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.TryAddScoped<IExpertSystemRepository, ExpertSystemRepository>();
            services.TryAddScoped<IConsultationDisplayProvider, DefaultConsultationDisplayProvider>();
            services.TryAddScoped<IAdviceRepository, AdviceRepository>();
            services.TryAddScoped<IFilmRecommendationService, FilmRecommendationService>();

            services.TryAddScoped<ConsultationExpertSystemResolver>();
            services.TryAddScoped<ConsultationShownFilmStore>();
            services.TryAddScoped<ConsultationReviewStore>();
            services.TryAddScoped<IConsultationWorkflow, ConsultationWorkflow>();

            services.AddSingleton<ConsultationExpertSystemCache>();
            services.AddScoped<ConsultationEngine>();
            services.AddScoped<ConsultationStateStore>();
            services.AddScoped<IConsultationService>(provider => new ConsultationService(
                provider.GetRequiredService<EsDbContext>(),
                provider.GetRequiredService<ConsultationEngine>(),
                provider.GetRequiredService<ConsultationStateStore>(),
                provider.GetRequiredService<ConsultationExpertSystemCache>()));

            return services;
        }
    }
}
