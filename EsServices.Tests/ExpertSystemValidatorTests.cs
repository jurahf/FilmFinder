using EsDb.Model.Enums;
using EsServices.Validators;
using Xunit;

namespace EsServices.Tests
{
    public class ExpertSystemValidatorTests
    {
        [Fact]
        public void ValidSimpleSystemPasses()
        {
            var validator = new ExpertSystemValidator();
            var result = validator.Validate(TestExpertSystemFactory.BuildSimple());

            Assert.True(result.Success, string.Join("; ", result.Errors.Select(x => x.Message)));
        }

        [Fact]
        public void QueriedVariableCannotBeRuleResult()
        {
            var expertSystem = TestExpertSystemFactory.BuildSimple();
            expertSystem.Rules[0].Result!.Variable = expertSystem.Variables[0];
            expertSystem.Rules[0].Result!.DomainValue = expertSystem.Domains[0].Values[0];

            var result = new ExpertSystemValidator().Validate(expertSystem);

            Assert.False(result.Success);
            Assert.Contains(result.Errors, x => x.Message.Contains("запрашиваемую переменную"));
        }

        [Fact]
        public void RuleWithoutResultFails()
        {
            var expertSystem = TestExpertSystemFactory.BuildSimple();
            expertSystem.Rules[0].Result = null;

            var result = new ExpertSystemValidator().Validate(expertSystem);

            Assert.False(result.Success);
            Assert.Contains(result.Errors, x => x.Message.Contains("не задан вывод"));
        }

        [Fact]
        public void DuplicateVariableNamesFail()
        {
            var expertSystem = TestExpertSystemFactory.BuildSimple();
            expertSystem.Variables[1].Name = expertSystem.Variables[0].Name;

            var result = new ExpertSystemValidator().Validate(expertSystem);

            Assert.False(result.Success);
            Assert.Contains(result.Errors, x => x.Message.Contains("уже существует"));
        }
    }
}
