using EsDb.Model.Enums;
using EsServices.Services.Export;
using EsServices.ViewModel;
using Xunit;

namespace EsServices.Tests
{
    public class ExpertSystemMermaidExporterTests
    {
        private static readonly DateTime ExportDate = new(2020, 1, 2, 3, 4, 5);

        [Fact]
        public void Export_ContainsQuestionChainAndLeaf()
        {
            var content = Export(TestExpertSystemFactory.BuildSimple());

            Assert.Contains("flowchart TD", content);
            Assert.Contains("Q1[\"Вопрос?\"]:::question", content);
            Assert.Contains("A1([\"Да\"]):::answer", content);
            Assert.Contains("A1 -->|\"1. R\"| P1", content);
            Assert.Contains("P1{{\"T = Да\"}}:::property", content);
        }

        [Fact]
        public void Export_AllAnswerOptionsAreShown()
        {
            var content = Export(TestExpertSystemFactory.BuildSimple());

            Assert.Contains("A1([\"Да\"]):::answer", content);
            Assert.Contains("A2([\"Нет\"]):::answer", content);
            Assert.Contains("Q1 --> A1", content);
            Assert.Contains("Q1 --> A2", content);
        }

        [Fact]
        public void Export_MultiConditionRuleBecomesChain()
        {
            var domain = YesNoDomain();
            var q1 = Queriable("Q1", "Вопрос?", domain);
            var q2 = Queriable("Q2", "Вопрос2?", domain);
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { q1, q2, target },
                Rules = { Rule(0, "R", Fact(target, "Да"), Fact(q1, "Да"), Fact(q2, "Да")) }
            };

            var content = Export(expertSystem);

            Assert.Contains("Q1[\"Вопрос?\"]:::question", content);
            Assert.Contains("A1([\"Да\"]):::answer", content);
            Assert.Contains("A1 --> Q2", content);
            Assert.Contains("Q2[\"Вопрос2?\"]:::question", content);
            Assert.Contains("Q2 --> A3", content);
            Assert.Contains("A3 -->|\"1. R\"| P1", content);
            Assert.DoesNotContain(":::rule", content);
        }

        [Fact]
        public void Export_SharesCommonPrefix()
        {
            var domain = YesNoDomain();
            var q1 = Queriable("Q1", "Вопрос?", domain);
            var q2 = Queriable("Q2", "Вопрос2?", domain);
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { q1, q2, target },
                Rules =
                {
                    Rule(0, "R1", Fact(target, "Да"), Fact(q1, "Да")),
                    Rule(1, "R2", Fact(target, "Нет"), Fact(q1, "Да"), Fact(q2, "Да"))
                }
            };

            var content = Export(expertSystem);

            Assert.Equal(1, CountOccurrences(content, "[\"Вопрос?\"]"));
            Assert.Equal(1, CountOccurrences(content, "A1([\"Да\"])"));
            Assert.Contains("A1 --> Q2", content);
        }

        [Fact]
        public void Export_DuplicatesQuestionInDifferentBranches()
        {
            var domain = YesNoDomain();
            var q1 = Queriable("Q1", "Вопрос?", domain);
            var q2 = Queriable("Q2", "Вопрос2?", domain);
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { q1, q2, target },
                Rules =
                {
                    Rule(0, "R1", Fact(target, "Да"), Fact(q1, "Да"), Fact(q2, "Да")),
                    Rule(1, "R2", Fact(target, "Нет"), Fact(q1, "Нет"), Fact(q2, "Да"))
                }
            };

            var content = Export(expertSystem);

            Assert.Equal(1, CountOccurrences(content, "[\"Вопрос?\"]"));
            Assert.Equal(2, CountOccurrences(content, "[\"Вопрос2?\"]"));
        }

        [Fact]
        public void Export_MergesSameLeaf()
        {
            var domain = YesNoDomain();
            var q1 = Queriable("Q1", "Вопрос?", domain);
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { q1, target },
                Rules =
                {
                    Rule(0, "R1", Fact(target, "Да"), Fact(q1, "Да")),
                    Rule(1, "R2", Fact(target, "Да"), Fact(q1, "Нет"))
                }
            };

            var content = Export(expertSystem);

            Assert.Equal(1, CountOccurrences(content, "{{\"T = Да\"}}"));
            Assert.Contains("|\"1. R1\"|", content);
            Assert.Contains("|\"2. R2\"|", content);
        }

        [Fact]
        public void Export_SupportsIntermediateFact()
        {
            var domain = YesNoDomain();
            var q1 = Queriable("Q1", "Вопрос?", domain);
            var intermediate = Detective("F");
            intermediate.Domain = domain;
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { q1, intermediate, target },
                Rules =
                {
                    Rule(0, "R1", Fact(intermediate, "Да"), Fact(q1, "Да")),
                    Rule(1, "R2", Fact(target, "Да"), Fact(intermediate, "Да"))
                }
            };

            var content = Export(expertSystem);

            Assert.Equal(1, CountOccurrences(content, "{{\"F = Да\"}}"));
            Assert.Contains("|\"1. R1\"|", content);
            Assert.Contains("|\"2. R2\"|", content);
            Assert.Contains("A1 -->|\"1. R1\"| P1", content);
            Assert.Contains("P1 -->|\"2. R2\"| P2", content);
        }

        [Fact]
        public void Export_EscapesQuotesAndLineBreaks()
        {
            var domain = YesNoDomain();
            var question = Queriable(
                "Q1",
                "Вопрос \"какой\"? #1 <нет>\nВторая & строка",
                domain);
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { question, target },
                Rules = { Rule(0, "R", Fact(target, "Да"), Fact(question, "Да")) }
            };

            var content = Export(expertSystem);

            Assert.Contains(
                "Q1[\"Вопрос #quot;какой#quot;? #35;1 #lt;нет#gt;<br/>Вторая #amp; строка\"]:::question",
                content);
        }

        [Fact]
        public void Export_RuleWithoutResult()
        {
            var domain = YesNoDomain();
            var question = Queriable("Q1", "Вопрос?", domain);
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { question, target },
                Rules =
                {
                    new RuleVM
                    {
                        Id = 1,
                        Order = 0,
                        Name = "R",
                        Result = null,
                        Conditions = { new RuleFactVM { Order = 0, Fact = Fact(question, "Да") } }
                    }
                }
            };

            var content = Export(expertSystem);

            Assert.Contains("X1{{\"Вывод не задан\"}}:::invalid", content);
            Assert.Contains("A1 -->|\"1. R\"| X1", content);
        }

        [Fact]
        public void Export_RuleWithoutConditions()
        {
            var domain = YesNoDomain();
            var target = Detective("T");

            var expertSystem = new ExpertSystemVM
            {
                Id = 1,
                Name = "Test",
                Target = target,
                Variables = { target },
                Rules = { Rule(0, "R", Fact(target, "Да")) }
            };

            var content = Export(expertSystem);

            Assert.Contains("X1{{\"1. R: нет посылок\"}}:::invalid", content);
        }

        [Fact]
        public void Export_HeaderContainsNameTargetAndDate()
        {
            var content = Export(TestExpertSystemFactory.BuildSimple());

            Assert.Contains("%% Экспертная система: Test (Id 1)", content);
            Assert.Contains("%% Дата экспорта: 2020-01-02", content);
            Assert.Contains("%% Цель консультации: T", content);
            Assert.Contains("%% Правил: 1", content);
        }

        private static string Export(ExpertSystemVM expertSystem)
        {
            return new ExpertSystemMermaidExporter().Export(expertSystem, ExportDate);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static DomainVM YesNoDomain()
        {
            return new DomainVM
            {
                Id = 1,
                Name = "ДаНет",
                Values =
                {
                    new DomainValueVM { Id = 1, Value = "Да", Display = "Да" },
                    new DomainValueVM { Id = 2, Value = "Нет", Display = "Нет" }
                }
            };
        }

        private static VariableVM Queriable(string name, string question, DomainVM domain)
        {
            return new VariableVM
            {
                Name = name,
                Question = question,
                Type = VariableType.Queriable,
                Domain = domain
            };
        }

        private static VariableVM Detective(string name)
        {
            return new VariableVM
            {
                Name = name,
                Type = VariableType.Detective
            };
        }

        private static FactVM Fact(VariableVM variable, string value)
        {
            var domainValue = variable.Domain?.Values.FirstOrDefault(x => x.Value == value)
                ?? new DomainValueVM { Value = value, Display = value };

            return new FactVM
            {
                Variable = variable,
                DomainValue = domainValue
            };
        }

        private static RuleVM Rule(int order, string name, FactVM? result, params FactVM[] conditions)
        {
            var rule = new RuleVM
            {
                Id = order + 1,
                Order = order,
                Name = name,
                Result = result
            };

            for (var i = 0; i < conditions.Length; i++)
                rule.Conditions.Add(new RuleFactVM { Order = i, Fact = conditions[i] });

            return rule;
        }
    }
}
