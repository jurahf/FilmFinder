using CommonRepositories;

namespace FilmDb.Model
{
    public class AdviceFilm : BaseEntity
    {
        public int Value { get; set; }

        public virtual Advice Advice { get; set; }

        public virtual int FilmId { get; set; }
        public virtual Film Film { get; set; }
    }
}
