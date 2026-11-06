using CommonRepositories;

namespace EsDb.Model
{
    public class RuleFact : BaseEntity
    {
        public int? Order { get; set; }

        public int RuleId { get; set; }
        public virtual Rule Rule { get; set; } = null!;

        public int FactId { get; set; }
        public virtual Fact Fact { get; set; } = null!;
    }
}
