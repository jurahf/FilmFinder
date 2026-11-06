using EsDb;
using EsDb.Repositories;
using EsServices.Converters;
using EsServices.Services;
using EsServices.Services.Advice;
using EsServices.Services.Consultation;
using EsServices.Validators;
using EsUi.Areas.Identity;
using EsUi.Data;
using FilmDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EsUi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection")
                ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
            var esConnectionString = builder.Configuration.GetConnectionString("EsConnection")
                ?? throw new InvalidOperationException("Connection string 'EsConnection' not found.");
            var filmConnectionString = builder.Configuration.GetConnectionString("FilmConnection")
                ?? throw new InvalidOperationException("Connection string 'FilmConnection' not found.");

            builder.Services.AddDbContext<EsDbContext>(options => options.UseSqlServer(esConnectionString));
            builder.Services.AddDbContext<FilmDbContext>(options => options.UseSqlServer(filmConnectionString));
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(identityConnectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            builder.Services.AddScoped<IExpertSystemRepository, ExpertSystemRepository>();
            builder.Services.AddScoped<ExpertSystemConverter>();
            builder.Services.AddScoped<ExpertSystemValidator>();
            builder.Services.AddScoped<IExpertSystemService, ExpertSystemService>();
            builder.Services.AddScoped<IAdviceCatalogService, AdviceCatalogService>();
            builder.Services.AddScoped<IAdviceSyncService, AdviceSyncService>();
            builder.Services.AddScoped<ConsultationEngine>();
            builder.Services.AddScoped<ConsultationSessionStore>();
            builder.Services.AddScoped<IConsultationService, ConsultationService>();
            builder.Services.AddScoped<IFilmRecommendationService, FilmRecommendationService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
