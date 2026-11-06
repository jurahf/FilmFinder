using CommonRepositories;

namespace EsDb.Model
{
    public class Fact : BaseEntity
    {
        public int VariableId { get; set; }
        public virtual Variable Variable { get; set; } = null!;

        public int? DomainValueId { get; set; }
        public virtual DomainValue? DomainValue { get; set; }
    }
}
