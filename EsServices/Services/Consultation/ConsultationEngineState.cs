using EsDb.Model.Enums;
using EsServices.ViewModel;
using EsServices.ViewModel.Consultation;

namespace EsServices.Services.Consultation
{
    public class ConsultationEngineState
    {
        public ConsultationEngineState(ExpertSystemVM expertSystem)
        {
            ExpertSystem = expertSystem;
        }

        public ExpertSystemVM ExpertSystem { get; }

        /// <summary>Стек целей: последний элемент — текущая цель.</summary>
        public List<VariableVM> GoalStack { get; } = new();

        /// <summary>Доказанные факты и их истинность.</summary>
        public Dictionary<FactVM, FactTruly> Proved { get; } = new();

        /// <summary>Факты, созданные из ответов пользователя и не имеющие записи в БД.</summary>
        public HashSet<FactVM> TransientFacts { get; } = new();

        /// <summary>Сработавшие и проверенные правила.</summary>
        public Dictionary<RuleVM, RuleWork> WorkedRules { get; } = new();

        public HashSet<VariableVM> UnknownVariables { get; } = new();

        public ConsultationResultVM? Result { get; set; }
    }
}
