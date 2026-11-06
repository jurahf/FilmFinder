using CommonRepositories;

namespace EsDb.Model
{
    public class FinalSolution : BaseEntity
    {
        public string VariableName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public int ConsultationId { get; set; }
        public virtual Consultation Consultation { get; set; } = null!;
    }
}
