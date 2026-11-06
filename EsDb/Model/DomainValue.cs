using CommonRepositories;

namespace EsDb.Model
{
    public class DomainValue : BaseEntity
    {
        public string Value { get; set; } = string.Empty;

        public int DomainId { get; set; }
        public virtual Domain Domain { get; set; } = null!;
    }
}
