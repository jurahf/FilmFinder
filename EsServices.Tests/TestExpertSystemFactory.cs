using EsDb.Model.Enums;
using EsServices.ViewModel;

namespace EsServices.Tests
{
    internal static class TestExpertSystemFactory
    {
        public static ExpertSystemVM BuildSimple(
            VariableType targetType = VariableType.Detective)
        {
            var yesNo = new DomainVM
            {
                Id = 1,
                Name = "ДаНет",
                Values =
                {
                    new DomainValueVM { Id = 1, Value = "Да", Display = "Да" },
                    new DomainValueVM { Id = 2, Value = "Нет", Display = "Нет" }
                }
            };

            var question = new VariableVM
            {
                Id = 1,
                Name = "Q",
                Question = "Вопрос?",
                Type = VariableType.Queriable,
                Domain = yesNo
            };

            var target = new VariableVM
            {
                Id = 2,
                Name = "T",
                Type = targetType,
                Domain = yesNo
            };

            var condition = new FactVM
            {
                Id = 1,
                Variable = question,
                DomainValue = yesNo.Values[0]
            };

            var result = new FactVM
            {
                Id = 2,
                Variable = target,
                DomainValue = yesNo.Values[0]
            };

            var rule = new RuleVM
            {
                Id = 1,
                Order = 0,
                Name = "R",
                Result = result,
                Conditions = { new RuleFactVM { Order = 0, Fact = condition } }
            };

            return new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Domains = { yesNo },
                Variables = { question, target },
                Rules = { rule },
                Facts = { condition, result }
            };
        }
    }
}
