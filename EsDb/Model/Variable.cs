using CommonRepositories;
using EsDb.Model.Enums;

namespace EsDb.Model
{
    public class Variable : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Question { get; set; }
        public string? Reasoning { get; set; }
        public VariableType Type { get; set; }

        public int? DomainId { get; set; }
        public virtual Domain? Domain { get; set; }

        public int ExpertSystemId { get; set; }
        public virtual ExpertSystem ExpertSystem { get; set; } = null!;
    }
}
