using EsDb;
using EsDb.Model;
using EsDb.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace EsServices.Tests
{
    internal static class ConsultationTestData
    {
        /// <summary>
        /// Создаёт ЭС: домен Да/Нет, вопрос Q, цель T, правило Q=Да -> T=Да.
        /// Факт для Q=Нет намеренно не создаётся, чтобы проверить транзитный ответ.
        /// </summary>
        public static async Task<int> SeedExpertSystemAsync(
            EsDbContext db,
            string name,
            VariableType targetType)
        {
            var yes = new DomainValue { Value = "Да" };
            var no = new DomainValue { Value = "Нет" };

            var domain = new Domain
            {
                Name = "ДаНет",
                DomainValue = { yes, no }
            };

            var question = new Variable
            {
                Name = "Q",
                Question = "Вопрос?",
                Type = VariableType.Queriable,
                Domain = domain
            };

            var target = new Variable
            {
                Name = "T",
                Type = targetType,
                Domain = domain
            };

            var condition = new Fact { Variable = question, DomainValue = yes };
            var result = new Fact { Variable = target, DomainValue = yes };

            var rule = new Rule
            {
                Order = 0,
                Name = "R",
                Result = result,
                Conditions = { new RuleFact { Order = 0, Fact = condition } }
            };

            var expertSystem = new ExpertSystem
            {
                Name = name,
                AllDomains = { domain },
                AllVariables = { question, target },
                AllRules = { rule }
            };

            domain.ExpertSystem = expertSystem;
            question.ExpertSystem = expertSystem;
            target.ExpertSystem = expertSystem;
            rule.ExpertSystem = expertSystem;

            // Сначала сохраняем граф без цели: ExpertSystem.TargetId ссылается
            // на Variable, и EF не может вставить обе стороны одним SaveChanges.
            db.ExpertSystems.Add(expertSystem);
            await db.SaveChangesAsync();

            expertSystem.TargetId = target.Id;
            await db.SaveChangesAsync();

            return expertSystem.Id;
        }
    }
}
