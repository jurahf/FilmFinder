using FilmDb.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb
{
    public class FilmDbContext : DbContext
    {
        public virtual DbSet<Film> Films { get; set; }
        public virtual DbSet<Country> Countrys { get; set; }
        public virtual DbSet<CountryFilm> CountryFilms { get; set; }
        public virtual DbSet<ActorFilm> ActorFilms { get; set; }
        public virtual DbSet<Actor> Actors { get; set; }
        public virtual DbSet<Producer> Producers { get; set; }
        public virtual DbSet<GenreFilm> GenreFilms { get; set; }
        public virtual DbSet<Genre> Genres { get; set; }
        public virtual DbSet<ProducerFilm> ProducerFilms { get; set; }
        public virtual DbSet<CustomProperty> CustomPropertys { get; set; }
        public virtual DbSet<FilmCustomProperty> FilmCustomPropertys { get; set; }


        public virtual DbSet<Advice> Advices { get; set; }
        public virtual DbSet<AdviceCustomProperty> AdviceCustomPropertys { get; set; }
        public virtual DbSet<AdviceFilm> AdviceFilms { get; set; }


        public virtual DbSet<IMDbLoading> IMDbLoadings { get; set; }


        public FilmDbContext()
            : base()
        {
        }

        public FilmDbContext(DbContextOptions<FilmDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<>()
            //    .HasKey(x => x.Id);
        }
    }
}
