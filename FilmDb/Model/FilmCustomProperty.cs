using CommonRepositories;
namespace FilmDb.Model
{
    public class FilmCustomProperty : BaseEntity
    {
        public int Value { get; set; }

        public virtual Film Film { get; set; }

        public virtual int CustomPropertyId { get; set; }
        public virtual CustomProperty CustomProperty { get; set; }
    }
}
