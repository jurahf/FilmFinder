using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class Film : BaseEntity
    {
        public Film()
        {
            this.CountryFilm = new HashSet<CountryFilm>();
            this.ActorFilm = new HashSet<ActorFilm>();
            this.GenreFilm = new HashSet<GenreFilm>();
            this.ProducerFilm = new HashSet<ProducerFilm>();
            this.FilmCustomProperty = new HashSet<FilmCustomProperty>();
        }

        public int Year { get; set; }
        public string Description { get; set; }
        public decimal? Rating { get; set; }
        public string Poster { get; set; }
        public string Slogan { get; set; }
        public string Link { get; set; }
        public string KinopoiskId { get; set; }
        public string Name { get; set; }


        public virtual ICollection<CountryFilm> CountryFilm { get; set; }

        public virtual ICollection<ActorFilm> ActorFilm { get; set; }

        public virtual ICollection<GenreFilm> GenreFilm { get; set; }

        public virtual ICollection<ProducerFilm> ProducerFilm { get; set; }

        public virtual ICollection<FilmCustomProperty> FilmCustomProperty { get; set; }
    }
}
