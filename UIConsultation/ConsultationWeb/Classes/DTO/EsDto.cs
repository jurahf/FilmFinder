using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConsultationWeb.Classes.DTO
{
    public class QuestionOrResultDto
    {
        public string SessionId { get; set; }
        public VariableDto Question { get; set; }
        public ConsultResultDto Result { get; set; }
    }

    public class ConsultResultDto
    {
        public FactDto Fact { get; set; }
        public List<RuleDto> Explain { get; set; }
    }


    public class FactDto
    {
        public string VarName { get; set; }
        public string Value { get; set; }
    }

    public class RuleDto
    {
        public List<string> Conditions { get; set; }
        public string Result { get; set; }
        public string Explaining { get; set; }
    }

    public class VariableDto
    {
        public string Name { get; set; }
        public string Question { get; set; }
        public List<string> Domain { get; set; }
    }
}