using EsDb.Model;
using Microsoft.EntityFrameworkCore;

namespace EsDb
{
    public class EsDbContext : DbContext
    {
        public virtual DbSet<ExpertSystem> ExpertSystems { get; set; }
        public virtual DbSet<Domain> Domains { get; set; }
        public virtual DbSet<DomainValue> DomainValues { get; set; }
        public virtual DbSet<Variable> Variables { get; set; }
        public virtual DbSet<Fact> Facts { get; set; }
        public virtual DbSet<Rule> Rules { get; set; }
        public virtual DbSet<RuleFact> RuleFacts { get; set; }
        public virtual DbSet<Consultation> Consultations { get; set; }
        public virtual DbSet<ConsultationFact> ConsultationFacts { get; set; }
        public virtual DbSet<ConsultationRule> ConsultationRules { get; set; }
        public virtual DbSet<GoalStack> GoalStacks { get; set; }
        public virtual DbSet<FinalSolution> FinalSolutions { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }

        public EsDbContext()
        {
        }

        public EsDbContext(DbContextOptions<EsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ExpertSystem>(entity =>
            {
                entity.HasIndex(x => x.Name);

                entity.HasMany(x => x.AllDomains)
                    .WithOne(x => x.ExpertSystem)
                    .HasForeignKey(x => x.ExpertSystemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.AllVariables)
                    .WithOne(x => x.ExpertSystem)
                    .HasForeignKey(x => x.ExpertSystemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.AllRules)
                    .WithOne(x => x.ExpertSystem)
                    .HasForeignKey(x => x.ExpertSystemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Target)
                    .WithMany()
                    .HasForeignKey(x => x.TargetId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Domain>(entity =>
            {
                entity.HasIndex(x => new { x.ExpertSystemId, x.Name });

                entity.HasMany(x => x.DomainValue)
                    .WithOne(x => x.Domain)
                    .HasForeignKey(x => x.DomainId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DomainValue>(entity =>
            {
                entity.HasIndex(x => new { x.DomainId, x.Value });
            });

            modelBuilder.Entity<Variable>(entity =>
            {
                entity.HasIndex(x => new { x.ExpertSystemId, x.Name });

                entity.HasOne(x => x.Domain)
                    .WithMany()
                    .HasForeignKey(x => x.DomainId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Fact>(entity =>
            {
                entity.HasIndex(x => new { x.VariableId, x.DomainValueId });

                entity.HasOne(x => x.Variable)
                    .WithMany()
                    .HasForeignKey(x => x.VariableId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.DomainValue)
                    .WithMany()
                    .HasForeignKey(x => x.DomainValueId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Rule>(entity =>
            {
                entity.HasIndex(x => new { x.ExpertSystemId, x.Name });

                entity.HasOne(x => x.Result)
                    .WithMany()
                    .HasForeignKey(x => x.ResultId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<RuleFact>(entity =>
            {
                entity.HasOne(x => x.Rule)
                    .WithMany(x => x.Conditions)
                    .HasForeignKey(x => x.RuleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Fact)
                    .WithMany()
                    .HasForeignKey(x => x.FactId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasIndex(x => x.SessionId).IsUnique();
            });

            modelBuilder.Entity<Consultation>(entity =>
            {
                entity.HasOne(x => x.Session)
                    .WithMany(x => x.Consultations)
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ExpertSystem)
                    .WithMany()
                    .HasForeignKey(x => x.ExpertSystemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CurrentRule)
                    .WithMany()
                    .HasForeignKey(x => x.CurrentRuleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.GoalStack)
                    .WithMany()
                    .HasForeignKey(x => x.GoalStackId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.FinalSolution)
                    .WithOne(x => x.Consultation)
                    .HasForeignKey<FinalSolution>(x => x.ConsultationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ConsultationFact>(entity =>
            {
                entity.HasOne(x => x.Consultation)
                    .WithMany(x => x.ProvedFacts)
                    .HasForeignKey(x => x.ConsultationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Fact)
                    .WithMany()
                    .HasForeignKey(x => x.FactId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ConsultationRule>(entity =>
            {
                entity.HasOne(x => x.Consultation)
                    .WithMany(x => x.WorkedRules)
                    .HasForeignKey(x => x.ConsultationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Rule)
                    .WithMany()
                    .HasForeignKey(x => x.RuleId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<GoalStack>(entity =>
            {
                entity.HasOne(x => x.Variable)
                    .WithMany()
                    .HasForeignKey(x => x.VariableId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.NextGoal)
                    .WithMany()
                    .HasForeignKey(x => x.NextGoalId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(x => x.Session)
                    .WithMany(x => x.Reviews)
                    .HasForeignKey(x => x.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
