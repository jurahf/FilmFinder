using EsConsultationServices.Contracts;
using EsConsultationServices.Engine;
using EsConsultationServices.Persistence;
using EsConsultationServices.Services;
using EsDb;
using EsDb.Repositories;
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
            services.AddMemoryCache();
            services.TryAddScoped<IExpertSystemRepository, ExpertSystemRepository>();
            services.TryAddScoped<IConsultationDisplayProvider, DefaultConsultationDisplayProvider>();

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
