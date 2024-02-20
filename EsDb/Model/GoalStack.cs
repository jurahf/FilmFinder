using CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb.Model
{
    public class GoalStack : BaseEntity
    {
        public virtual Variable Variable { get; set; }
        public virtual GoalStack NextGoal { get; set; }
    }
}
