using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertSystemDb.DTOs
{
    public class FactQuestionOrResult
    {
        public Variable Question { get; set; }
        public ConsultationFact Result { get; set; }
    }

    public class RuleQuestionOrResult
    {
        public Variable Question { get; set; }
        public ConsultationRule Result { get; set; }
    }




    public class QuestionOrResultDto
    {
        public string SessionId { get; set; }
        public VariableDto Question { get; set; }
        public ConsultResultDto Result { get; set; }

        public QuestionOrResultDto(string sessionId)
        {
            this.SessionId = sessionId;
        }
    }

    public class ConsultResultDto
    {
        public FactDto Fact { get; set; }
        public List<RuleDto> Explain { get; set; }

        public ConsultResultDto(Fact fact, List<ConsultationRule> rules)
        {
            Fact = new FactDto(fact);
            Explain = rules.Select(x => new RuleDto(x.Rule)).ToList();
        }
    }


    public class FactDto
    {
        public string VarName { get; set; }
        public string Value { get; set; }

        public FactDto(Fact fact)
        {
            VarName = fact.Variable.Name;
            Value = fact.DomainValue.Value;
        }
    }

    public class RuleDto
    {
        public List<string> Conditions { get; set; }
        public string Result { get; set; }
        public string Explaining { get; set; }

        public RuleDto(Rule rule)
        {
            Conditions = rule.Conditions.Select(r => r.ToString()).ToList();
            Result = rule.Result.ToString();
            Explaining = rule.Reasoning;
        }
    }

    public class VariableDto
    {
        public string Name { get; set; }
        public string Question { get; set; }
        public List<string> Domain { get; set; }

        public VariableDto(Variable variable)
        {
            Name = variable.Name;
            Question = variable.Question;
            Domain = variable.Domain.DomainValue.Select(x => x.Value).ToList();
        }
    }
}
