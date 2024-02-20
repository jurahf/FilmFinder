using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmDb.Model
{
    public class IMDbLoading : BaseEntity
    {
        public string DataSetID { get; set; }
        public string IMDbId { get; set; }
        public string EnglishTitle { get; set; }
        public string Year { get; set; }
        public string EnglishGenries { get; set; }
        public string EnglishDescription { get; set; }
        public string Poster { get; set; }
        public string EnglishActors { get; set; }
        public string EnglishCountries { get; set; }
        public string EnglishProducers { get; set; }
        public string Rating { get; set; }
        public string EnglishTags { get; set; }
        public string RussianTitle { get; set; }
        public string RussianGenries { get; set; }
        public string RussianDescription { get; set; }
        public string RussianActors { get; set; }
        public string RussianCountries { get; set; }
        public string RussianProducers { get; set; }
        public string RussianTags { get; set; }
    }
}
