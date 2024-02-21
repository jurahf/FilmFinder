using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.ViewModel
{
    public class FilmVM : BaseViewModel
    {
        public FilmVM()
        {
            this.Countries = new HashSet<CountryVM>();
            this.Actors= new HashSet<ActorVM>();
            this.Genres = new HashSet<GenreVM>();
            this.Producers = new HashSet<ProducerVM>();
            this.CustomProperties = new HashSet<CustomPropertyVM>();
        }

        public int Year { get; set; }
        public string? Description { get; set; }
        public decimal? Rating { get; set; }
        public string? Poster { get; set; }
        public string? Slogan { get; set; }
        public string? Link { get; set; }
        public string? KinopoiskId { get; set; }
        public string? Name { get; set; }


        public virtual ICollection<CountryVM> Countries { get; set; }

        public virtual ICollection<ActorVM> Actors { get; set; }

        public virtual ICollection<GenreVM> Genres { get; set; }

        public virtual ICollection<ProducerVM> Producers { get; set; }

        public virtual ICollection<CustomPropertyVM> CustomProperties { get; set; }
    }
}
