using CommonRepositories;

namespace FilmDb.Model
{
    public class CountryFilm : BaseEntity
    {
        public virtual Film Film { get; set; }

        public virtual int CountryId { get; set; }
        public virtual Country Country { get; set; }
    }
}
