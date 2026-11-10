using CommonRepositories;

namespace EsDb.Model
{
    /// <summary>
    /// Переменная, которую не удалось вывести в ходе консультации.
    /// Нужна для восстановления объяснения после перезапуска.
    /// </summary>
    public class ConsultationUnknownVariable : BaseEntity
    {
        public int ConsultationId { get; set; }
        public virtual Consultation Consultation { get; set; } = null!;

        public int VariableId { get; set; }
        public virtual Variable Variable { get; set; } = null!;
    }
}
