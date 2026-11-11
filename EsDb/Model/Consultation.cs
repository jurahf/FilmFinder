using CommonRepositories;

namespace EsDb.Model
{
    public class Consultation : BaseEntity
    {
        public Consultation()
        {
            ProvedFacts = new HashSet<ConsultationFact>();
            WorkedRules = new HashSet<ConsultationRule>();
            Answers = new HashSet<ConsultationAnswer>();
            UnknownVariables = new HashSet<ConsultationUnknownVariable>();
            ShownFilms = new HashSet<ConsultationShownFilm>();
        }

        public int SessionId { get; set; }
        public virtual Session Session { get; set; } = null!;

        public int ExpertSystemId { get; set; }
        public virtual ExpertSystem ExpertSystem { get; set; } = null!;

        public int? CurrentRuleId { get; set; }
        public virtual Rule? CurrentRule { get; set; }

        public int? GoalStackId { get; set; }
        public virtual GoalStack? GoalStack { get; set; }

        public virtual ICollection<ConsultationFact> ProvedFacts { get; set; }
        public virtual ICollection<ConsultationRule> WorkedRules { get; set; }
        public virtual ICollection<ConsultationAnswer> Answers { get; set; }
        public virtual ICollection<ConsultationUnknownVariable> UnknownVariables { get; set; }
        public virtual ICollection<ConsultationShownFilm> ShownFilms { get; set; }
        public virtual FinalSolution? FinalSolution { get; set; }
    }
}
