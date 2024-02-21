using CommonRepositories;
using FilmDb;
using FilmDb.Model;
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


            builder.Services.AddScoped<IRepository<Film>>(sp => new BaseRepository<Film>(sp.GetService<FilmDbContext>()));
            builder.Services.AddScoped<IEntityConverter<Film, FilmVM>, FilmConverter>();
            builder.Services.AddScoped<IValidator<FilmVM>>(sp => new DefaultValidator<FilmVM>());
            builder.Services.AddScoped<IService<Film, FilmVM>>(sp => new BaseSevice<Film, FilmVM>(
                sp.GetService<IRepository<Film>>(),
                sp.GetService<IEntityConverter<Film, FilmVM>>(),
                sp.GetService<IValidator<FilmVM>>()
                ));





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