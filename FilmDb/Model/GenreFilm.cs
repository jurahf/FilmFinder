using CommonRepositories;

namespace FilmDb.Model
{
    public class GenreFilm : BaseEntity
    {
        public virtual Film Film { get; set; }

        public virtual int GenreId { get; set; }
        public virtual Genre Genre { get; set; }
    }
}
