using EsConsultationServices.Engine;
using EsDb.Model.Enums;
using Xunit;

namespace EsServices.Tests
{
    public class ConsultationEngineTests
    {
        [Fact]
        public void AsksQuestionThenReturnsResult()
        {
            var engine = new ConsultationEngine();
            var state = engine.Start(TestConsultationGraphFactory.BuildSimple());

            var step = engine.Next(state);

            Assert.NotNull(step.Question);
            Assert.Equal("Q", step.Question!.Name);
            Assert.Equal("Вопрос?", step.Question.Text);
            Assert.Equal(2, step.Question.Answers.Count);

            var result = engine.Answer(state, "Q", "Да");

            Assert.NotNull(result.Result);
            Assert.False(result.Result!.IsUnknown);
            Assert.Equal("T", result.Result.VariableName);
            Assert.Equal("Да", result.Result.Value);
            Assert.Contains(result.Result.Explanation.Rules,
                x => x.Name == "R" && x.Work == RuleWork.Signify);
        }

        [Fact]
        public void ReturnsUnknownWhenConditionIsFalse()
        {
            var engine = new ConsultationEngine();
            var state = engine.Start(TestConsultationGraphFactory.BuildSimple());

            var step = engine.Next(state);
            Assert.NotNull(step.Question);

            var result = engine.Answer(state, "Q", "Нет");

            Assert.NotNull(result.Result);
            Assert.True(result.Result!.IsUnknown);
            Assert.Contains(result.Result.Explanation.Rules,
                x => x.Name == "R" && x.Work == RuleWork.UnSignify);
        }

        [Fact]
        public void DeductionQueriedAsksIfRulesFail()
        {
            var engine = new ConsultationEngine();
            var state = engine.Start(
                TestConsultationGraphFactory.BuildSimple(VariableType.QuerriableDetective));

            var step = engine.Next(state);
            Assert.NotNull(step.Question);
            Assert.Equal("Q", step.Question!.Name);

            step = engine.Answer(state, "Q", "Нет");

            Assert.NotNull(step.Question);
            Assert.Equal("T", step.Question!.Name);

            var result = engine.Answer(state, "T", "Нет");

            Assert.NotNull(result.Result);
            Assert.False(result.Result!.IsUnknown);
            Assert.Equal("Нет", result.Result.Value);
        }
    }
}
