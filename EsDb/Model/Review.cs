using CommonRepositories;

namespace EsDb.Model
{
    public class Review : BaseEntity
    {
        public int Estimate { get; set; }
        public string? Comment { get; set; }

        public int SessionId { get; set; }
        public virtual Session Session { get; set; } = null!;
    }
}
