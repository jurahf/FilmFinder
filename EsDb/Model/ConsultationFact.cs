using CommonRepositories;
using EsDb.Model.Enums;

namespace EsDb.Model
{
    public class ConsultationFact : BaseEntity
    {
        public FactTruly Truly { get; set; }

        public int ConsultationId { get; set; }
        public virtual Consultation Consultation { get; set; } = null!;

        public int FactId { get; set; }
        public virtual Fact Fact { get; set; } = null!;
    }
}
