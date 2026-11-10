using CommonRepositories;

namespace EsDb.Model
{
    /// <summary>
    /// Ответ пользователя в рамках консультации.
    /// Хранится отдельно от ConsultationFact, потому что для ответа может не быть
    /// заранее созданной записи Fact (например, значение есть в домене, но факт не описан).
    /// </summary>
    public class ConsultationAnswer : BaseEntity
    {
        public string Value { get; set; } = string.Empty;

        /// <summary>Порядковый номер ответа в консультации, начиная с 0.</summary>
        public int Order { get; set; }

        public int ConsultationId { get; set; }
        public virtual Consultation Consultation { get; set; } = null!;

        public int VariableId { get; set; }
        public virtual Variable Variable { get; set; } = null!;

        public int? DomainValueId { get; set; }
        public virtual DomainValue? DomainValue { get; set; }
    }
}
