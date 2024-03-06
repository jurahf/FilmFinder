using CommonRepositories;

namespace FilmDb.Model
{
    public class ProducerFilm : BaseEntity
    {
        public virtual int ProducerId { get; set; }
        public virtual Producer Producer { get; set; }
        public virtual Film Film { get; set; }
    }
}
