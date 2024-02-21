using CommonRepositories;
using FilmDb;
using FilmDb.Model;
using FilmDb.Repositories;
using FilmsServices.Converters;
using FilmsServices.Converters.Common;
using FilmsServices.Services.Common;
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

            builder.Services.AddScoped<IRepository<Actor>>(sp => new BaseRepository<Actor>(sp.GetService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Country>>(sp => new BaseRepository<Country>(sp.GetService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Genre>>(sp => new BaseRepository<Genre>(sp.GetService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Producer>>(sp => new BaseRepository<Producer>(sp.GetService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<CustomProperty>>(sp => new BaseRepository<CustomProperty>(sp.GetService<FilmDbContext>()));
            builder.Services.AddScoped<IRepository<Advice>, AdviceRepository>();
            builder.Services.AddScoped<IRepository<Film>, FilmRepository>();


            builder.Services.AddScoped<IValidator<ActorVM>>(sp => new DefaultValidator<ActorVM>());
            builder.Services.AddScoped<IValidator<CountryVM>>(sp => new DefaultValidator<CountryVM>());
            builder.Services.AddScoped<IValidator<GenreVM>>(sp => new DefaultValidator<GenreVM>());
            builder.Services.AddScoped<IValidator<ProducerVM>>(sp => new DefaultValidator<ProducerVM>());
            builder.Services.AddScoped<IValidator<CustomPropertyVM>>(sp => new DefaultValidator<CustomPropertyVM>());
            builder.Services.AddScoped<IValidator<AdviceVM>>(sp => new DefaultValidator<AdviceVM>());
            builder.Services.AddScoped<IValidator<FilmVM>>(sp => new DefaultValidator<FilmVM>());

            builder.Services.AddScoped<IService<Actor, ActorVM>, BaseSevice<Actor, ActorVM>>();
            builder.Services.AddScoped<IService<Country, CountryVM>, BaseSevice<Country, CountryVM>>();
            builder.Services.AddScoped<IService<Genre, GenreVM>, BaseSevice<Genre, GenreVM>>();
            builder.Services.AddScoped<IService<Producer, ProducerVM>, BaseSevice<Producer, ProducerVM>>();
            builder.Services.AddScoped<IService<CustomProperty, CustomPropertyVM>, BaseSevice<CustomProperty, CustomPropertyVM>>();
            builder.Services.AddScoped<IService<Advice, AdviceVM>, BaseSevice<Advice, AdviceVM>>();
            builder.Services.AddScoped<IService<Film, FilmVM>, BaseSevice<Film, FilmVM>>();



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