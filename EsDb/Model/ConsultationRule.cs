using CommonRepositories;
using EsDb.Model.Enums;

namespace EsDb.Model
{
    public class ConsultationRule : BaseEntity
    {
        public RuleWork Work { get; set; }

        public int ConsultationId { get; set; }
        public virtual Consultation Consultation { get; set; } = null!;

        public int RuleId { get; set; }
        public virtual Rule Rule { get; set; } = null!;
    }
}
