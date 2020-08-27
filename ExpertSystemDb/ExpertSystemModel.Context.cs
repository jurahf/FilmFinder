﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExpertSystemDb
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ExpertSystemModelContainer : DbContext
    {
        public ExpertSystemModelContainer()
            : base("name=ExpertSystemModelContainer")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ExpertSystem> ExpertSystemSet { get; set; }
        public virtual DbSet<Domain> DomainSet { get; set; }
        public virtual DbSet<DomainValue> DomainValueSet { get; set; }
        public virtual DbSet<Variable> VariableSet { get; set; }
        public virtual DbSet<Fact> FactSet { get; set; }
        public virtual DbSet<Rule> RuleSet { get; set; }
        public virtual DbSet<RuleFact> RuleFactSet { get; set; }
        public virtual DbSet<Consultation> ConsultationSet { get; set; }
        public virtual DbSet<Session> SessionSet { get; set; }
        public virtual DbSet<ConsultationFact> ConsultationFactSet { get; set; }
        public virtual DbSet<ConsultationRule> ConsultationRuleSet { get; set; }
        public virtual DbSet<GoalStack> GoalStackSet { get; set; }
        public virtual DbSet<Review> ReviewSet { get; set; }
        public virtual DbSet<Film> FilmSet { get; set; }
        public virtual DbSet<Country> CountrySet { get; set; }
        public virtual DbSet<CountryFilm> CountryFilmSet { get; set; }
        public virtual DbSet<ActorFilm> ActorFilmSet { get; set; }
        public virtual DbSet<Actor> ActorSet { get; set; }
        public virtual DbSet<Producer> ProducerSet { get; set; }
        public virtual DbSet<GenreFilm> GenreFilmSet { get; set; }
        public virtual DbSet<Genre> GenreSet { get; set; }
        public virtual DbSet<ProducerFilm> ProducerFilmSet { get; set; }
        public virtual DbSet<CustomProperty> CustomPropertySet { get; set; }
        public virtual DbSet<FilmCustomProperty> FilmCustomPropertySet { get; set; }
        public virtual DbSet<Advice> AdviceSet { get; set; }
        public virtual DbSet<AdviceCustomProperty> AdviceCustomPropertySet { get; set; }
        public virtual DbSet<AdviceFilm> AdviceFilmSet { get; set; }
        public virtual DbSet<IMDbLoading> IMDbLoadingSet { get; set; }
    }
}
