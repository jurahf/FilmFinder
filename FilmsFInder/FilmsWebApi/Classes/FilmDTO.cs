using FilmsWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FilmsWebApi.Classes
{
    public class FilmDTO
    {
        public int Id { get; set; }

        public string KinopoiskId { get; set; }

        public string Name { get; set; }

        public int Year { get; set; }

        public string Description { get; set; }

        public decimal Rating { get; set; }

        public string Poster { get; set; }

        public string Slogan { get; set; }

        public string Link { get; set; }

        public ProducerDTO Producer { get; set; }

        public List<ActorDTO> Actors { get; set; }

        public List<CountryDTO> Countries { get; set; }

        public List<GenreDTO> Genres { get; set; }

        public FilmDTO()
        {
        }

        public FilmDTO(Film film)
        {
            this.Id = film.Id;
            this.KinopoiskId = film.KinopoiskId;
            this.Name = film.Name;
            this.Year = film.Year;
            this.Description = film.Description;
            this.Rating = film.Rating ?? 0;
            this.Poster = film.Poster;
            this.Slogan = film.Slogan;
            this.Link = film.Link;

            this.Producer = film.Producer == null 
                ? null 
                : new ProducerDTO(film.Producer);

            this.Actors = film.ActorFilm == null
                ? new List<ActorDTO>()
                : film.ActorFilm.Select(x => new ActorDTO(x.Actor)).ToList();

            this.Countries = film.CountryFilm == null
                ? new List<CountryDTO>()
                : film.CountryFilm.Select(x => new CountryDTO(x.Country)).ToList();

            this.Genres = film.GenreFilm == null
                ? new List<GenreDTO>()
                : film.GenreFilm.Select(x => new GenreDTO(x.Genre)).ToList();
        }
    }

    public class ProducerDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ProducerDTO(Producer producer)
        {
            this.Id = producer.Id;
            this.Name = producer.Name;
        }
    }

    public class ActorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ActorDTO(Actor actor)
        {
            this.Id = actor.Id;
            this.Name = actor.Name;
        }
    }

    public class CountryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CountryDTO(Country country)
        {
            this.Id = country.Id;
            this.Name = country.Name;
        }
    }

    public class GenreDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public GenreDTO(Genre genre)
        {
            this.Id = genre.Id;
            this.Name = genre.Name;
        }
    }

}