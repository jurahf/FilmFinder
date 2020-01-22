using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsultationWeb.Classes.DTO
{
    public class AdvicePreviewDto
    {
        public List<FilmPreviewDto> Films { get; set; }
    }

    public class FilmPreviewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosterUrl { get; set; }
        public decimal Rating { get; set; }
    }


    public class AdviceDto
    {
        public List<FilmDto> Films { get; set; }
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
    }


    public class ActorDto
    {
        public string Name { get; set; }
    }

    public class CountryDto
    {
        public string Name { get; set; }
    }

    public class GenreDto
    {
        public string Name { get; set; }
    }

    public class ProducerDto
    {
        public string Name { get; set; }
    }
}