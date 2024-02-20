using EsDb.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsDb
{
    public class EsDbContext : DbContext
    {
        public virtual DbSet<ExpertSystem> ExpertSystems { get; set; }
        public virtual DbSet<Domain> Domains { get; set; }
        public virtual DbSet<DomainValue> DomainValues { get; set; }
        public virtual DbSet<Variable> Variables { get; set; }
        public virtual DbSet<Fact> Facts{ get; set; }
        public virtual DbSet<Rule> Rules { get; set; }
        public virtual DbSet<RuleFact> RuleFacts { get; set; }
        public virtual DbSet<Consultation> Consultations { get; set; }
        public virtual DbSet<ConsultationFact> ConsultationFacts { get; set; }
        public virtual DbSet<ConsultationRule> ConsultationRules { get; set; }
        public virtual DbSet<GoalStack> GoalStacks { get; set; }
        public virtual DbSet<FinalSolution> FinalSolutions { get; set; }


        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        
        

        //public virtual DbSet<PreprocessQuestions> PreprocessQuestionsSet { get; set; } // Filter
        //public virtual DbSet<GenreForFilter> GenreForFilterSet { get; set; }
        //public virtual DbSet<CustomPropertyForFilter> CustomPropertyForFilterSet { get; set; }


        public EsDbContext()
            : base()
        {
        }

        public EsDbContext(DbContextOptions<EsDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<>()
            //    .HasKey(x => x.Id);
        }
    }
}
