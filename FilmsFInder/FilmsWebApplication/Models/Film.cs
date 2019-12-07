using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FilmsWebApplication.Models
{
    public class Film
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

        public Producer Producer { get; set; }

        public List<Actor> Actors { get; set; }

        public List<Country> Countries { get; set; }

        public List<Genre> Genres { get; set; }
    }

    public class Producer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}