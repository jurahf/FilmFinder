using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicClasses;
using ExpertSystemDb;

namespace EsStorageAdapter
{
    public interface IDbToClassicConverter
    {
        ClassicClasses.ExpertSystem ESConvert(ExpertSystemDb.ExpertSystem fromDb, List<Consultation> colsultationList);
        ClassicClasses.ValueDomain DomainConvert(ExpertSystemDb.Domain fromDb);
        ClassicClasses.Variable VariableConvert(ExpertSystemDb.Variable fromDb, OrderedDictionary<string, ClassicClasses.ValueDomain> domains);
        ClassicClasses.Fact FactConvert(ExpertSystemDb.Fact fromDb, OrderedDictionary<string, ClassicClasses.Variable> variables, ref List<ClassicClasses.Fact> existedFacts);
        ClassicClasses.Rule RuleConvert(ExpertSystemDb.Rule fromDb, OrderedDictionary<string, ClassicClasses.Variable> variables, ref List<ClassicClasses.Fact> existedFacts);
    }

    public class DbToClassicConverter : IDbToClassicConverter
    {
        public ClassicClasses.ExpertSystem ESConvert(ExpertSystemDb.ExpertSystem fromDb, List<Consultation> consultationList)
        {
            ClassicClasses.ExpertSystem classic = new ClassicClasses.ExpertSystem();

            // из ЭС
            foreach (var domain in fromDb.AllDomains)
            {
                classic.Domains.Add(domain.Name, DomainConvert(domain));
            }

            foreach (var variable in fromDb.AllVariables)
            {
                classic.Vars.Add(variable.Name, VariableConvert(variable, classic.Domains));
            }

            List<ClassicClasses.Fact> existedFacts = new List<ClassicClasses.Fact>();
            foreach (var rule in fromDb.AllRules.OrderBy(x => x.Order))
            {
                classic.Rules.Add(rule.Name, RuleConvert(rule, classic.Vars, ref existedFacts));
            }

            classic.Goal = FindVariable(classic.Vars, fromDb.Target);

            // из консультации (пока берем из последней)
            Consultation consult = consultationList
                .OrderByDescending(x => x.Session.CreateDate)
                .FirstOrDefault();
            // TODO: 
            //Proved
            //WorkedRules


            return classic;
        }


        public ClassicClasses.ValueDomain DomainConvert(ExpertSystemDb.Domain fromDb)
        {
            ClassicClasses.ValueDomain result = new ClassicClasses.ValueDomain();
            result.Name = fromDb.Name;
            result.ListVal = fromDb.DomainValue.Select(x => x.Value).ToList();

            return result;
        }

        public ClassicClasses.Variable VariableConvert(ExpertSystemDb.Variable fromDb, OrderedDictionary<string, ClassicClasses.ValueDomain> domains)
        {
            ClassicClasses.Variable result = new ClassicClasses.Variable();
            result.Name = fromDb.Name;
            result.Question = fromDb.Question;
            result.Reasoning = fromDb.Reasoning;
            result.MyType = VariableTypeConvert(fromDb.Type);
            result.Domain = domains[fromDb.Domain.Name];  // catch exception ?

            return result;
        }

        private VarType VariableTypeConvert(VariableType type)
        {
            switch (type)
            {
                case VariableType.Queriable:
                    return VarType.Queried;
                case VariableType.Detective:
                    return VarType.Deducted;
                case VariableType.QuerriableDetective:
                    return VarType.DeductionQueried;
                default:
                    throw new ArgumentException($"Не найден тип переменной {type}");
            }
        }


        public ClassicClasses.Rule RuleConvert(ExpertSystemDb.Rule fromDb, OrderedDictionary<string, ClassicClasses.Variable> variables, ref List<ClassicClasses.Fact> existedFacts)
        {
            ClassicClasses.Rule result = new ClassicClasses.Rule();
            result.Name = fromDb.Name;
            result.Reasoning = fromDb.Reasoning;
            result.Worked = ClassicClasses.RuleWork.No;
            result.Result = FactConvert(fromDb.Result, variables, ref existedFacts);
            result.Reasons = new List<ClassicClasses.Fact>();
            foreach (var ruleFact in fromDb.Conditions)
            {
                result.Reasons.Add(FactConvert(ruleFact.Fact, variables, ref existedFacts));
            }

            return result;
        }

        public ClassicClasses.Fact FactConvert(ExpertSystemDb.Fact fromDb, OrderedDictionary<string, ClassicClasses.Variable> variables, ref List<ClassicClasses.Fact> existedFacts)
        {
            ClassicClasses.Variable variable = FindVariable(variables, fromDb.Variable);
            ClassicClasses.Fact finded = FindFact(existedFacts, variable, fromDb.DomainValue?.Value);
            if (finded != null)
            {
                return finded;
            }
            else
            {
                ClassicClasses.Fact result = new ClassicClasses.Fact();
                result.V = variable;
                result.Truly = Rightly.Unknown;
                result.Weight = fromDb.DomainValue.Value; // variable уже должно быть присвоено

                existedFacts.Add(result);
                return result;
            }
        }

        private ClassicClasses.Fact FindFact(List<ClassicClasses.Fact> existedFacts, ClassicClasses.Variable variable, string value)
        {
            if (string.IsNullOrEmpty(value) || variable == null)
                return null;

            return existedFacts.FirstOrDefault(x => x.V.Name == variable.Name && x.Weight == value);
        }

        private ClassicClasses.Variable FindVariable(OrderedDictionary<string, ClassicClasses.Variable> variablesList, ExpertSystemDb.Variable variable)
        {
            if (variable == null)
                return null;

            return variablesList[variable.Name];    // TODO: catch exceptions
        }



    }
}
