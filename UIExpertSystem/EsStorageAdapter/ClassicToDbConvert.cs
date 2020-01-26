using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicClasses;
using ExpertSystemDb;

namespace EsStorageAdapter
{
    public interface IClassicToDbConvert
    {
        ExpertSystemDb.ExpertSystem ESConvert(ClassicClasses.ExpertSystem classic, string name);
        ExpertSystemDb.Domain DomainConvert(ClassicClasses.ValueDomain classic);
        ExpertSystemDb.Variable VariableConvert(ClassicClasses.Variable classic, ICollection<Domain> domains);
        ExpertSystemDb.Fact FactConvert(ClassicClasses.Fact classic, ICollection<ExpertSystemDb.Variable> variables, ref List<ExpertSystemDb.Fact> existedFacts);
        ExpertSystemDb.Rule RuleConvert(ClassicClasses.Rule classic, int order, ICollection<ExpertSystemDb.Variable> variables, ref List<ExpertSystemDb.Fact> existedFacts);
    }

    public class ClassicToDbConvert : IClassicToDbConvert
    {
        public ExpertSystemDb.ExpertSystem ESConvert(ClassicClasses.ExpertSystem classic, string name)
        {
            ExpertSystemDb.ExpertSystem es = new ExpertSystemDb.ExpertSystem();
            es.Name = name;

            foreach (var domain in classic.Domains)
            {
                es.AllDomains.Add(DomainConvert(classic.Domains[domain]));
            }

            foreach (var variable in classic.Vars)
            {
                es.AllVariables.Add(VariableConvert(classic.Vars[variable], es.AllDomains));
            }

            int i = 0;
            List<ExpertSystemDb.Fact> existedFacts = new List<ExpertSystemDb.Fact>();
            foreach (var rule in classic.Rules)
            {
                es.AllRules.Add(RuleConvert(classic.Rules[rule], i, es.AllVariables, ref existedFacts));
                i++;
            }

            es.Target = FindVariable(classic.Goal, es.AllVariables);

            // консультации не берем, они не проводятся теперь в Classic-классах

            return es;
        }


        public ExpertSystemDb.Domain DomainConvert(ClassicClasses.ValueDomain classic)
        {
            ExpertSystemDb.Domain result = new ExpertSystemDb.Domain();
            result.Name = classic.Name;
            result.DomainValue = classic.ListVal
                .Select(x => new ExpertSystemDb.DomainValue() { Value = x })
                .ToList();

            return result;
        }


        public ExpertSystemDb.Variable VariableConvert(ClassicClasses.Variable classic, ICollection<Domain> domains)
        {
            ExpertSystemDb.Variable result = new ExpertSystemDb.Variable();
            result.Name = classic.Name;
            result.Question = classic.Question;
            result.Reasoning = classic.Reasoning;
            result.Type = VaiableTypeConvert(classic.MyType);
            result.Domain = domains.FirstOrDefault(x => x.Name == classic.Domain.Name); // catch exceptions

            return result;
        }

        private VariableType VaiableTypeConvert(VarType myType)
        {
            switch (myType)
            {
                case VarType.Deducted:
                    return VariableType.Detective;
                case VarType.Queried:
                    return VariableType.Queriable;
                case VarType.DeductionQueried:
                    return VariableType.QuerriableDetective;
                default:
                    throw new ArgumentException($"Не найден тип переменной {myType}");
            }
        }


        public ExpertSystemDb.Rule RuleConvert(ClassicClasses.Rule classic, int order, ICollection<ExpertSystemDb.Variable> variables, ref List<ExpertSystemDb.Fact> existedFacts)
        {
            ExpertSystemDb.Rule result = new ExpertSystemDb.Rule();
            result.Name = classic.Name;
            result.Reasoning = classic.Reasoning;
            result.Order = order;
            result.Result = FactConvert(classic.Result, variables, ref existedFacts);
            for (int i = 0; i < classic.Reasons.Count; i++)
            {
                var condition = classic.Reasons[i];
                result.Conditions.Add(new RuleFact()
                {
                    Order = i + 1,  // вообще-то List не дает гарантии, что элементы будут в одном порядке всегда
                    Rule = result,
                    Fact = FactConvert(condition, variables, ref existedFacts)
                });
            }

            return result;
        }


        public ExpertSystemDb.Fact FactConvert(ClassicClasses.Fact classic, ICollection<ExpertSystemDb.Variable> variables, ref List<ExpertSystemDb.Fact> existedFacts)
        {
            ExpertSystemDb.Variable variable = FindVariable(classic.V, variables);
            ExpertSystemDb.Fact finded = FindFact(existedFacts, variable, classic.Weight);

            if (finded != null)
            {
                return finded;
            }
            else
            {
                ExpertSystemDb.Fact result = new ExpertSystemDb.Fact();
                result.Variable = variable;
                result.DomainValue = variable.Domain.DomainValue.First(x => x.Value == classic.Weight);

                return result;
            }
        }

        private ExpertSystemDb.Fact FindFact(List<ExpertSystemDb.Fact> existedFacts, ExpertSystemDb.Variable variable, string value)
        {
            if (variable == null || string.IsNullOrEmpty(value))
                return null;

            return existedFacts.FirstOrDefault(x => x.Variable.Name == variable.Name && x.DomainValue?.Value == value);
        }

        private ExpertSystemDb.Variable FindVariable(ClassicClasses.Variable v, ICollection<ExpertSystemDb.Variable> variables)
        {
            if (v == null)
                return null;

            return variables.FirstOrDefault(x => x.Name == v.Name);
        }





    }
}
