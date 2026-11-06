using EsDb.Model;
using EsServices.ViewModel;

namespace EsServices.Converters
{
    public class ExpertSystemConverter
    {
        public const string AdviceDomainName = "#Советы";

        public ExpertSystemVM ConvertToVm(
            ExpertSystem database,
            IEnumerable<Fact> facts,
            IReadOnlyDictionary<string, string> adviceDescriptions)
        {
            var result = new ExpertSystemVM
            {
                Id = database.Id,
                Name = database.Name
            };

            var domains = new Dictionary<int, DomainVM>();
            foreach (var dbDomain in database.AllDomains.OrderBy(x => x.Id))
            {
                var domainVm = new DomainVM
                {
                    Id = dbDomain.Id,
                    Name = dbDomain.Name
                };

                foreach (var dbValue in dbDomain.DomainValue.OrderBy(x => x.Id))
                {
                    domainVm.Values.Add(new DomainValueVM
                    {
                        Id = dbValue.Id,
                        Value = dbValue.Value,
                        Display = ResolveDisplay(dbDomain.Name, dbValue.Value, adviceDescriptions)
                    });
                }

                domains[dbDomain.Id] = domainVm;
                result.Domains.Add(domainVm);
            }

            var variables = new Dictionary<int, VariableVM>();
            foreach (var dbVariable in database.AllVariables.OrderBy(x => x.Id))
            {
                var variableVm = new VariableVM
                {
                    Id = dbVariable.Id,
                    Name = dbVariable.Name,
                    Question = dbVariable.Question,
                    Reasoning = dbVariable.Reasoning,
                    Type = dbVariable.Type
                };

                if (dbVariable.DomainId.HasValue && domains.TryGetValue(dbVariable.DomainId.Value, out var domain))
                    variableVm.Domain = domain;

                variables[dbVariable.Id] = variableVm;
                result.Variables.Add(variableVm);
            }

            var values = domains.Values
                .SelectMany(x => x.Values)
                .ToDictionary(x => x.Id);

            var factVms = new Dictionary<int, FactVM>();
            foreach (var dbFact in facts.OrderBy(x => x.Id))
            {
                if (!variables.TryGetValue(dbFact.VariableId, out var variable))
                    continue;

                DomainValueVM? value = null;
                if (dbFact.DomainValueId.HasValue)
                    values.TryGetValue(dbFact.DomainValueId.Value, out value);

                if (value == null)
                    continue;

                var factVm = new FactVM
                {
                    Id = dbFact.Id,
                    Variable = variable,
                    DomainValue = value
                };

                factVms[dbFact.Id] = factVm;
                result.Facts.Add(factVm);
            }

            foreach (var dbRule in database.AllRules.OrderBy(x => x.Order).ThenBy(x => x.Id))
            {
                var ruleVm = new RuleVM
                {
                    Id = dbRule.Id,
                    Order = dbRule.Order,
                    Name = dbRule.Name,
                    Reasoning = dbRule.Reasoning
                };

                foreach (var dbCondition in dbRule.Conditions.OrderBy(x => x.Order ?? int.MaxValue).ThenBy(x => x.Id))
                {
                    if (factVms.TryGetValue(dbCondition.FactId, out var fact))
                    {
                        ruleVm.Conditions.Add(new RuleFactVM
                        {
                            Id = dbCondition.Id,
                            Order = dbCondition.Order ?? 0,
                            Fact = fact
                        });
                    }
                }

                if (dbRule.ResultId.HasValue && factVms.TryGetValue(dbRule.ResultId.Value, out var ruleResult))
                    ruleVm.Result = ruleResult;

                result.Rules.Add(ruleVm);
            }

            if (database.TargetId.HasValue && variables.TryGetValue(database.TargetId.Value, out var target))
                result.Target = target;

            return result;
        }

        /// <summary>
        /// Создаёт новый граф EF-сущностей для сохранения новой версией.
        /// Цель (Target) намеренно не устанавливается: из-за циклической связи
        /// её нужно проставить после первого SaveChanges.
        /// </summary>
        public ExpertSystem CreateNewDbGraph(ExpertSystemVM viewModel, string name)
        {
            var expertSystem = new ExpertSystem
            {
                Name = name
            };

            var domains = new Dictionary<DomainVM, Domain>();
            foreach (var domainVm in viewModel.Domains)
            {
                var domain = new Domain
                {
                    Name = domainVm.Name,
                    ExpertSystem = expertSystem
                };

                foreach (var valueVm in domainVm.Values)
                {
                    domain.DomainValue.Add(new DomainValue
                    {
                        Value = valueVm.Value,
                        Domain = domain
                    });
                }

                domains[domainVm] = domain;
                expertSystem.AllDomains.Add(domain);
            }

            var variables = new Dictionary<VariableVM, Variable>();
            foreach (var variableVm in viewModel.Variables)
            {
                var variable = new Variable
                {
                    Name = variableVm.Name,
                    Question = variableVm.Question,
                    Reasoning = variableVm.Reasoning,
                    Type = variableVm.Type,
                    ExpertSystem = expertSystem
                };

                if (variableVm.Domain != null && domains.TryGetValue(variableVm.Domain, out var domain))
                    variable.Domain = domain;

                variables[variableVm] = variable;
                expertSystem.AllVariables.Add(variable);
            }

            var facts = new Dictionary<(string Variable, string Value), Fact>();

            Fact GetOrCreateFact(FactVM factVm)
            {
                var key = (factVm.Variable.Name, factVm.DomainValue.Value);
                if (facts.TryGetValue(key, out var existed))
                    return existed;

                if (!variables.TryGetValue(factVm.Variable, out var variable))
                    throw new InvalidOperationException($"Не найдена переменная '{factVm.Variable.Name}' при сохранении правила.");

                var domainValue = variable.Domain?.DomainValue
                    .FirstOrDefault(x => x.Value == factVm.DomainValue.Value);

                if (domainValue == null)
                    throw new InvalidOperationException(
                        $"Значение '{factVm.DomainValue.Value}' не найдено в домене переменной '{factVm.Variable.Name}'.");

                var fact = new Fact
                {
                    Variable = variable,
                    DomainValue = domainValue
                };

                facts[key] = fact;
                return fact;
            }

            int ruleOrder = 0;
            foreach (var ruleVm in viewModel.Rules)
            {
                var rule = new Rule
                {
                    Order = ruleOrder++,
                    Name = ruleVm.Name,
                    Reasoning = ruleVm.Reasoning,
                    ExpertSystem = expertSystem
                };

                int conditionOrder = 0;
                foreach (var condition in ruleVm.Conditions)
                {
                    rule.Conditions.Add(new RuleFact
                    {
                        Order = conditionOrder++,
                        Rule = rule,
                        Fact = GetOrCreateFact(condition.Fact)
                    });
                }

                if (ruleVm.Result != null)
                    rule.Result = GetOrCreateFact(ruleVm.Result);

                expertSystem.AllRules.Add(rule);
            }

            return expertSystem;
        }

        private static string ResolveDisplay(
            string domainName,
            string value,
            IReadOnlyDictionary<string, string> adviceDescriptions)
        {
            if (!string.Equals(domainName, AdviceDomainName, StringComparison.Ordinal))
                return value;

            if (Guid.TryParse(value, out var key) &&
                adviceDescriptions.TryGetValue(key.ToString("D"), out var description) &&
                !string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            return value;
        }
    }
}
