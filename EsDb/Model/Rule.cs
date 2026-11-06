using CommonRepositories;

namespace EsDb.Model
{
    public class Rule : BaseEntity
    {
        public Rule()
        {
            Conditions = new HashSet<RuleFact>();
        }

        public int Order { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Reasoning { get; set; }

        public int? ResultId { get; set; }
        public virtual Fact? Result { get; set; }

        public int ExpertSystemId { get; set; }
        public virtual ExpertSystem ExpertSystem { get; set; } = null!;

        public virtual ICollection<RuleFact> Conditions { get; set; }
    }
}
