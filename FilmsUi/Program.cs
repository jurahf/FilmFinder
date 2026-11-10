using CommonRepositories;
using FilmDb;
using FilmDb.Model;
using FilmDb.Repositories;
using FilmsServices.Converters;
using FilmsServices.Converters.Common;
using FilmsServices.Services;
using FilmsServices.Services.Common;
using FilmsServices.Services.Kinopoisk;
using FilmsServices.Validators.Common;
using FilmsServices.ViewModel;
using FilmsUi.Areas.Identity;
using FilmsUi.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;

namespace FilmsUi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection") 
                ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
            var filmConnectionString = builder.Configuration.GetConnectionString("FilmConnection")
                ?? throw new InvalidOperationException("Connection string 'FilmConnection' not found.");

            builder.Services.AddDbContext<FilmDbContext>(options => options.UseSqlServer(filmConnectionString));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(identityConnectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();


            builder.Services.AddScoped<IEntityConverter<Actor, ActorVM>>(sp => new DefaultConverter<Actor, ActorVM>());
            builder.Services.AddScoped<IEntityConverter<Country, CountryVM>>(sp => new DefaultConverter<Country, CountryVM>());
            builder.Services.AddScoped<IEntityConverter<Genre, GenreVM>>(sp => new DefaultConverter<Genre, GenreVM>());
            builder.Services.AddScoped<IEntityConverter<Producer, ProducerVM>>(sp => new DefaultConverter<Producer, ProducerVM>());
            builder.Services.AddScoped<IEntityConverter<CustomProperty, CustomPropertyVM>>(sp => new DefaultConverter<CustomProperty, CustomPropertyVM>());
            builder.Services.AddScoped<IEntityConverter<Advice, AdviceVM>, AdviceConverter>();
            builder.Services.AddScoped<IEntityConverter<Film, FilmVM>, FilmConverter>();

            builder.Services.AddScoped<IRepository<Actor>>(sp => new NamedRepository<Actor>(sp.GetRequiredService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Country>>(sp => new NamedRepository<Country>(sp.GetRequiredService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Genre>>(sp => new NamedRepository<Genre>(sp.GetRequiredService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Producer>>(sp => new NamedRepository<Producer>(sp.GetRequiredService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<CustomProperty>>(sp => new NamedRepository<CustomProperty>(sp.GetRequiredService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Advice>, AdviceRepository>();
            builder.Services.AddScoped<IFilmRepository, FilmRepository>();


            builder.Services.AddScoped<IValidator<ActorVM>>(sp => new DefaultValidator<ActorVM>());
            builder.Services.AddScoped<IValidator<CountryVM>>(sp => new DefaultValidator<CountryVM>());
            builder.Services.AddScoped<IValidator<GenreVM>>(sp => new DefaultValidator<GenreVM>());
            builder.Services.AddScoped<IValidator<ProducerVM>>(sp => new DefaultValidator<ProducerVM>());
            builder.Services.AddScoped<IValidator<CustomPropertyVM>>(sp => new DefaultValidator<CustomPropertyVM>());
            builder.Services.AddScoped<IValidator<AdviceVM>>(sp => new DefaultValidator<AdviceVM>());
            builder.Services.AddScoped<IValidator<FilmVM>>(sp => new DefaultValidator<FilmVM>());

            builder.Services.AddScoped<IService<ActorVM>, NamedService<Actor, ActorVM>>();
            builder.Services.AddScoped<INamedService<ActorVM>, NamedService<Actor, ActorVM>>();
            builder.Services.AddScoped<IService<CountryVM>, NamedService<Country, CountryVM>>();
            builder.Services.AddScoped<INamedService<CountryVM>, NamedService<Country, CountryVM>>();
            builder.Services.AddScoped<IService<GenreVM>, NamedService<Genre, GenreVM>>();
            builder.Services.AddScoped<INamedService<GenreVM>, NamedService<Genre, GenreVM>>();
            builder.Services.AddScoped<IService<ProducerVM>, NamedService<Producer, ProducerVM>>();
            builder.Services.AddScoped<INamedService<ProducerVM>, NamedService<Producer, ProducerVM>>();
            builder.Services.AddScoped<IService<CustomPropertyVM>, NamedService<CustomProperty, CustomPropertyVM>>();
            builder.Services.AddScoped<INamedService<CustomPropertyVM>, NamedService<CustomProperty, CustomPropertyVM>>();
            builder.Services.AddScoped<IService<AdviceVM>, BaseSevice<Advice, AdviceVM>>();
            builder.Services.AddScoped<IFilmService, FilmService>();

            builder.Services.AddSingleton(new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(30)
            });
            builder.Services.AddSingleton<KinopoiskHtmlParser>();
            builder.Services.AddScoped<IKinopoiskFilmImportService, KinopoiskFilmImportService>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}