using CommonRepositories;

namespace EsDb.Model
{
    public class ExpertSystem : BaseEntity
    {
        public ExpertSystem()
        {
            AllDomains = new HashSet<Domain>();
            AllVariables = new HashSet<Variable>();
            AllRules = new HashSet<Rule>();
        }

        public string Name { get; set; } = string.Empty;

        public int? TargetId { get; set; }
        public virtual Variable? Target { get; set; }

        public virtual ICollection<Domain> AllDomains { get; set; }
        public virtual ICollection<Variable> AllVariables { get; set; }
        public virtual ICollection<Rule> AllRules { get; set; }
    }
}
