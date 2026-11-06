using CommonRepositories;

namespace EsDb.Model
{
    public class GoalStack : BaseEntity
    {
        public int? VariableId { get; set; }
        public virtual Variable? Variable { get; set; }

        public int? NextGoalId { get; set; }
        public virtual GoalStack? NextGoal { get; set; }
    }
}
