using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Classes
{
    public class AnswerEsArgs
    {
        public string SessionId { get; set; }
        public VariableValue VarValue { get; set; }
    }


}