using CommonRepositories;

namespace EsDb.Model
{
    public class Domain : BaseEntity
    {
        public Domain()
        {
            DomainValue = new HashSet<DomainValue>();
        }

        public string Name { get; set; } = string.Empty;

        public int ExpertSystemId { get; set; }
        public virtual ExpertSystem ExpertSystem { get; set; } = null!;

        public virtual ICollection<DomainValue> DomainValue { get; set; }
    }
}
