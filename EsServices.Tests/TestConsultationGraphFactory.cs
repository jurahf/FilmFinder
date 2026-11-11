using EsConsultationServices.Engine;
using EsDb.Model.Enums;

namespace EsServices.Tests
{
    internal static class TestConsultationGraphFactory
    {
        public static ConsultationExpertSystem BuildSimple(
            VariableType targetType = VariableType.Detective)
        {
            var yes = new ConsultationDomainValue { Id = 1, Value = "Да", Display = "Да" };
            var no = new ConsultationDomainValue { Id = 2, Value = "Нет", Display = "Нет" };

            var yesNo = new ConsultationDomain
            {
                Id = 1,
                Name = "ДаНет",
                Values = new List<ConsultationDomainValue> { yes, no }
            };

            var question = new ConsultationVariable
            {
                Id = 1,
                Name = "Q",
                Question = "Вопрос?",
                Type = VariableType.Queriable,
                Domain = yesNo
            };

            var target = new ConsultationVariable
            {
                Id = 2,
                Name = "T",
                Type = targetType,
                Domain = yesNo
            };

            var condition = new ConsultationFact
            {
                Id = 1,
                Variable = question,
                DomainValue = yes
            };

            var result = new ConsultationFact
            {
                Id = 2,
                Variable = target,
                DomainValue = yes
            };

            var rule = new ConsultationRule
            {
                Id = 1,
                Order = 0,
                Name = "R",
                Result = result,
                Conditions = new List<ConsultationRuleFact>
                {
                    new() { Order = 0, Fact = condition }
                }
            };

            return new ConsultationExpertSystem
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = new List<ConsultationVariable> { question, target },
                Rules = new List<ConsultationRule> { rule },
                Facts = new List<ConsultationFact> { condition, result }
            };
        }
    }
}
