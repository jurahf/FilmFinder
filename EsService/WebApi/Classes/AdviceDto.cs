using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Classes
{
    public class AdvicePreviewDto
    {
        public List<FilmPreviewDto> Films { get; set; }

        public AdvicePreviewDto(Advice advice)
        {
            Films = new List<FilmPreviewDto>();

            foreach (var filmAdvice in advice.AdviceFilmPositive)
            {
                Films.Add(new FilmPreviewDto(filmAdvice.Film));
            }
        }
    }

    public class FilmPreviewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public decimal Rating { get; set; }

        public FilmPreviewDto(Film film)
        {
            this.Id = film.Id;
            this.Name = film.Name;
            this.PosterUrl = film.Poster;
            this.Rating = film.Rating ?? 0;
        }
    }


    public class AdviceDto
    {
        public List<FilmDto> Films { get; set; }
        // TODO: custom property

        public AdviceDto(Advice advice)
        {
            Films = new List<FilmDto>();

            foreach (var filmAdvice in advice.AdviceFilmPositive)
            {
                Films.Add(new FilmDto(filmAdvice.Film));
            }
        }
    }

    public class FilmDto
    {
        public List<ActorDto> Actors { get; set; }
        public List<CountryDto> Countries { get; set; }
        public string Description { get; set; }
        //film.FilmCustomProperty
        public List<GenreDto> Genries { get; set; }
        public int Id { get; set; }
        public string KinopoiskId { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public List<ProducerDto> Producers { get; set; }
        public decimal Rating { get; set; }
        public string Slogan { get; set; }
        public int Year { get; set; }

        public FilmDto(Film film)
        {
            this.Id = film.Id;
            this.Name = film.Name;
            this.Description = film.Description;
            this.Year = film.Year;
            this.KinopoiskId = film.KinopoiskId;
            this.Link = film.Link;
            this.Rating = film.Rating ?? 0;
            this.Slogan = film.Slogan;
            this.PosterUrl = film.Poster;
            //film.FilmCustomProperty
            this.Producers = new List<ProducerDto>();
            foreach (var prodFilm in film.ProducerFilm)
            {
                this.Producers.Add(new ProducerDto(prodFilm.Producer));
            }
            this.Actors = new List<ActorDto>();
            foreach (var actorFilm in film.ActorFilm)
            {
                this.Actors.Add(new ActorDto(actorFilm.Actor));
            }
            this.Countries = new List<CountryDto>();
            foreach (var countFilm in film.CountryFilm)
            {
                this.Countries.Add(new CountryDto(countFilm.Country));
            }
            this.Genries = new List<GenreDto>();
            foreach (var genreFilm in film.GenreFilm)
            {
                this.Genries.Add(new GenreDto(genreFilm.Genre));
            }
        }
    }


    public class ActorDto
    {
        public string Name { get; set; }

        public ActorDto(Actor actor)
        {
            this.Name = actor.Name;
        }
    }

    public class CountryDto
    {
        public string Name { get; set; }

        public CountryDto(Country country)
        {
            this.Name = country.Name;
        }
    }

    public class GenreDto
    {
        public string Name { get; set; }

        public GenreDto(Genre genre)
        {
            this.Name = genre.Name;
        }
    }

    public class ProducerDto
    {
        public string Name { get; set; }

        public ProducerDto(Producer producer)
        {
            this.Name = producer.Name;
        }
    }

}