
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/02/2020 11:55:24
-- Generated from EDMX file: D:\PROJECTS\CSharp\FilmFinder\ExpertSystemDb\ExpertSystemModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ExpertSystemDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_ExpertSystemDomain]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DomainSet] DROP CONSTRAINT [FK_ExpertSystemDomain];
GO
IF OBJECT_ID(N'[dbo].[FK_DomainDomainValue]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DomainValueSet] DROP CONSTRAINT [FK_DomainDomainValue];
GO
IF OBJECT_ID(N'[dbo].[FK_DomainVariable]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[VariableSet] DROP CONSTRAINT [FK_DomainVariable];
GO
IF OBJECT_ID(N'[dbo].[FK_ExpertSystemVariable]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[VariableSet] DROP CONSTRAINT [FK_ExpertSystemVariable];
GO
IF OBJECT_ID(N'[dbo].[FK_ExpertSystemVariable1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[VariableSet] DROP CONSTRAINT [FK_ExpertSystemVariable1];
GO
IF OBJECT_ID(N'[dbo].[FK_DomainValueFact]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FactSet] DROP CONSTRAINT [FK_DomainValueFact];
GO
IF OBJECT_ID(N'[dbo].[FK_VariableFact]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FactSet] DROP CONSTRAINT [FK_VariableFact];
GO
IF OBJECT_ID(N'[dbo].[FK_FactRule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RuleSet] DROP CONSTRAINT [FK_FactRule];
GO
IF OBJECT_ID(N'[dbo].[FK_RuleRuleFact]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RuleFactSet] DROP CONSTRAINT [FK_RuleRuleFact];
GO
IF OBJECT_ID(N'[dbo].[FK_FactRuleFact]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RuleFactSet] DROP CONSTRAINT [FK_FactRuleFact];
GO
IF OBJECT_ID(N'[dbo].[FK_ExpertSystemRule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RuleSet] DROP CONSTRAINT [FK_ExpertSystemRule];
GO
IF OBJECT_ID(N'[dbo].[FK_SessionConsultation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SessionSet] DROP CONSTRAINT [FK_SessionConsultation];
GO
IF OBJECT_ID(N'[dbo].[FK_ExpertSystemConsultation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationSet] DROP CONSTRAINT [FK_ExpertSystemConsultation];
GO
IF OBJECT_ID(N'[dbo].[FK_ConsultationConsultationFact]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationFactSet] DROP CONSTRAINT [FK_ConsultationConsultationFact];
GO
IF OBJECT_ID(N'[dbo].[FK_ConsultationConsultationRule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationRuleSet] DROP CONSTRAINT [FK_ConsultationConsultationRule];
GO
IF OBJECT_ID(N'[dbo].[FK_FactConsultationFact]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationFactSet] DROP CONSTRAINT [FK_FactConsultationFact];
GO
IF OBJECT_ID(N'[dbo].[FK_RuleConsultationRule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationRuleSet] DROP CONSTRAINT [FK_RuleConsultationRule];
GO
IF OBJECT_ID(N'[dbo].[FK_RuleConsultation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationSet] DROP CONSTRAINT [FK_RuleConsultation];
GO
IF OBJECT_ID(N'[dbo].[FK_GoalStackConsultation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ConsultationSet] DROP CONSTRAINT [FK_GoalStackConsultation];
GO
IF OBJECT_ID(N'[dbo].[FK_VariableGoalStack]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GoalStackSet] DROP CONSTRAINT [FK_VariableGoalStack];
GO
IF OBJECT_ID(N'[dbo].[FK_GoalStackGoalStack]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GoalStackSet] DROP CONSTRAINT [FK_GoalStackGoalStack];
GO
IF OBJECT_ID(N'[dbo].[FK_SessionReview]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReviewSet] DROP CONSTRAINT [FK_SessionReview];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmCountryFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CountryFilmSet] DROP CONSTRAINT [FK_FilmCountryFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmActorFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActorFilmSet] DROP CONSTRAINT [FK_FilmActorFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmGenreFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GenreFilmSet] DROP CONSTRAINT [FK_FilmGenreFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_CountryCountryFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CountryFilmSet] DROP CONSTRAINT [FK_CountryCountryFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_ActorActorFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActorFilmSet] DROP CONSTRAINT [FK_ActorActorFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_GenreGenreFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GenreFilmSet] DROP CONSTRAINT [FK_GenreGenreFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_ProducerProducerFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProducerFilmSet] DROP CONSTRAINT [FK_ProducerProducerFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmProducerFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProducerFilmSet] DROP CONSTRAINT [FK_FilmProducerFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmFilmCustomProperty]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FilmCustomPropertySet] DROP CONSTRAINT [FK_FilmFilmCustomProperty];
GO
IF OBJECT_ID(N'[dbo].[FK_CustomPropertyFilmCustomProperty]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FilmCustomPropertySet] DROP CONSTRAINT [FK_CustomPropertyFilmCustomProperty];
GO
IF OBJECT_ID(N'[dbo].[FK_CustomPropertyAdviceCustomProperty]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AdviceCustomPropertySet] DROP CONSTRAINT [FK_CustomPropertyAdviceCustomProperty];
GO
IF OBJECT_ID(N'[dbo].[FK_AdviceAdviceFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AdviceFilmSet] DROP CONSTRAINT [FK_AdviceAdviceFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmAdviceFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AdviceFilmSet] DROP CONSTRAINT [FK_FilmAdviceFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_AdviceAdviceCustomProperty]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AdviceCustomPropertySet] DROP CONSTRAINT [FK_AdviceAdviceCustomProperty];
GO
IF OBJECT_ID(N'[dbo].[FK_PreprocessQuestionsSession]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PreprocessQuestionsSet] DROP CONSTRAINT [FK_PreprocessQuestionsSession];
GO
IF OBJECT_ID(N'[dbo].[FK_PreprocessQuestionsGenreForFilter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GenreForFilterSet] DROP CONSTRAINT [FK_PreprocessQuestionsGenreForFilter];
GO
IF OBJECT_ID(N'[dbo].[FK_GenreGenreForFilter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GenreForFilterSet] DROP CONSTRAINT [FK_GenreGenreForFilter];
GO
IF OBJECT_ID(N'[dbo].[FK_PreprocessQuestionsCustomPropertyForFilter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CustomPropertyForFilterSet] DROP CONSTRAINT [FK_PreprocessQuestionsCustomPropertyForFilter];
GO
IF OBJECT_ID(N'[dbo].[FK_CustomPropertyCustomPropertyForFilter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CustomPropertyForFilterSet] DROP CONSTRAINT [FK_CustomPropertyCustomPropertyForFilter];
GO
IF OBJECT_ID(N'[dbo].[FK_FinalSolutionConsultation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FinalSolutionSet] DROP CONSTRAINT [FK_FinalSolutionConsultation];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[ExpertSystemSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ExpertSystemSet];
GO
IF OBJECT_ID(N'[dbo].[DomainSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DomainSet];
GO
IF OBJECT_ID(N'[dbo].[DomainValueSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DomainValueSet];
GO
IF OBJECT_ID(N'[dbo].[VariableSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[VariableSet];
GO
IF OBJECT_ID(N'[dbo].[FactSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FactSet];
GO
IF OBJECT_ID(N'[dbo].[RuleSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RuleSet];
GO
IF OBJECT_ID(N'[dbo].[RuleFactSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RuleFactSet];
GO
IF OBJECT_ID(N'[dbo].[ConsultationSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ConsultationSet];
GO
IF OBJECT_ID(N'[dbo].[SessionSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SessionSet];
GO
IF OBJECT_ID(N'[dbo].[ConsultationFactSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ConsultationFactSet];
GO
IF OBJECT_ID(N'[dbo].[ConsultationRuleSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ConsultationRuleSet];
GO
IF OBJECT_ID(N'[dbo].[GoalStackSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GoalStackSet];
GO
IF OBJECT_ID(N'[dbo].[ReviewSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReviewSet];
GO
IF OBJECT_ID(N'[dbo].[FilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FilmSet];
GO
IF OBJECT_ID(N'[dbo].[CountrySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CountrySet];
GO
IF OBJECT_ID(N'[dbo].[CountryFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CountryFilmSet];
GO
IF OBJECT_ID(N'[dbo].[ActorFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ActorFilmSet];
GO
IF OBJECT_ID(N'[dbo].[ActorSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ActorSet];
GO
IF OBJECT_ID(N'[dbo].[ProducerSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProducerSet];
GO
IF OBJECT_ID(N'[dbo].[GenreFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GenreFilmSet];
GO
IF OBJECT_ID(N'[dbo].[GenreSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GenreSet];
GO
IF OBJECT_ID(N'[dbo].[ProducerFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProducerFilmSet];
GO
IF OBJECT_ID(N'[dbo].[CustomPropertySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CustomPropertySet];
GO
IF OBJECT_ID(N'[dbo].[FilmCustomPropertySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FilmCustomPropertySet];
GO
IF OBJECT_ID(N'[dbo].[AdviceSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AdviceSet];
GO
IF OBJECT_ID(N'[dbo].[AdviceCustomPropertySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AdviceCustomPropertySet];
GO
IF OBJECT_ID(N'[dbo].[AdviceFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AdviceFilmSet];
GO
IF OBJECT_ID(N'[dbo].[IMDbLoadingSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[IMDbLoadingSet];
GO
IF OBJECT_ID(N'[dbo].[PreprocessQuestionsSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PreprocessQuestionsSet];
GO
IF OBJECT_ID(N'[dbo].[GenreForFilterSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GenreForFilterSet];
GO
IF OBJECT_ID(N'[dbo].[CustomPropertyForFilterSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CustomPropertyForFilterSet];
GO
IF OBJECT_ID(N'[dbo].[FinalSolutionSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FinalSolutionSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'ExpertSystemSet'
CREATE TABLE [dbo].[ExpertSystemSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'DomainSet'
CREATE TABLE [dbo].[DomainSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [ExpertSystemDomain_Domain_Id] int  NOT NULL
);
GO

-- Creating table 'DomainValueSet'
CREATE TABLE [dbo].[DomainValueSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] nvarchar(max)  NOT NULL,
    [DomainDomainValue_DomainValue_Id] int  NOT NULL
);
GO

-- Creating table 'VariableSet'
CREATE TABLE [dbo].[VariableSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Question] nvarchar(max)  NOT NULL,
    [Reasoning] nvarchar(max)  NOT NULL,
    [Type] int  NOT NULL,
    [Domain_Id] int  NOT NULL,
    [ExpertSystemVariable_Variable_Id] int  NOT NULL,
    [ExpertSystemVariable1_Variable_Id] int  NULL
);
GO

-- Creating table 'FactSet'
CREATE TABLE [dbo].[FactSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DomainValue_Id] int  NOT NULL,
    [Variable_Id] int  NOT NULL
);
GO

-- Creating table 'RuleSet'
CREATE TABLE [dbo].[RuleSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Order] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Reasoning] nvarchar(max)  NOT NULL,
    [Result_Id] int  NOT NULL,
    [ExpertSystemRule_Rule_Id] int  NOT NULL
);
GO

-- Creating table 'RuleFactSet'
CREATE TABLE [dbo].[RuleFactSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Order] int  NULL,
    [Rule_Id] int  NOT NULL,
    [Fact_Id] int  NOT NULL
);
GO

-- Creating table 'ConsultationSet'
CREATE TABLE [dbo].[ConsultationSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Session_Id] int  NOT NULL,
    [ExpertSystem_Id] int  NOT NULL,
    [CurrentRule_Id] int  NULL,
    [GoalStack_Id] int  NULL
);
GO

-- Creating table 'SessionSet'
CREATE TABLE [dbo].[SessionSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SessionId] nvarchar(max)  NOT NULL,
    [CreateDate] datetime  NOT NULL,
    [LastActivityDate] datetime  NULL
);
GO

-- Creating table 'ConsultationFactSet'
CREATE TABLE [dbo].[ConsultationFactSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Truly] int  NOT NULL,
    [Consultation_Id] int  NOT NULL,
    [Fact_Id] int  NOT NULL
);
GO

-- Creating table 'ConsultationRuleSet'
CREATE TABLE [dbo].[ConsultationRuleSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Work] int  NOT NULL,
    [Consultation_Id] int  NOT NULL,
    [Rule_Id] int  NOT NULL
);
GO

-- Creating table 'GoalStackSet'
CREATE TABLE [dbo].[GoalStackSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Variable_Id] int  NULL,
    [NextGoal_Id] int  NULL
);
GO

-- Creating table 'ReviewSet'
CREATE TABLE [dbo].[ReviewSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Estimate] int  NOT NULL,
    [Comment] nvarchar(max)  NULL,
    [Session_Id] int  NOT NULL
);
GO

-- Creating table 'FilmSet'
CREATE TABLE [dbo].[FilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Year] int  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [Rating] decimal(11,9)  NULL,
    [Poster] nvarchar(max)  NULL,
    [Slogan] nvarchar(max)  NULL,
    [Link] nvarchar(max)  NULL,
    [KinopoiskId] nvarchar(max)  NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'CountrySet'
CREATE TABLE [dbo].[CountrySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'CountryFilmSet'
CREATE TABLE [dbo].[CountryFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Film_Id] int  NOT NULL,
    [Country_Id] int  NOT NULL
);
GO

-- Creating table 'ActorFilmSet'
CREATE TABLE [dbo].[ActorFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Film_Id] int  NOT NULL,
    [Actor_Id] int  NOT NULL
);
GO

-- Creating table 'ActorSet'
CREATE TABLE [dbo].[ActorSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ProducerSet'
CREATE TABLE [dbo].[ProducerSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GenreFilmSet'
CREATE TABLE [dbo].[GenreFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Film_Id] int  NOT NULL,
    [Genre_Id] int  NOT NULL
);
GO

-- Creating table 'GenreSet'
CREATE TABLE [dbo].[GenreSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ProducerFilmSet'
CREATE TABLE [dbo].[ProducerFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Producer_Id] int  NOT NULL,
    [Film_Id] int  NOT NULL
);
GO

-- Creating table 'CustomPropertySet'
CREATE TABLE [dbo].[CustomPropertySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'FilmCustomPropertySet'
CREATE TABLE [dbo].[FilmCustomPropertySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] int  NOT NULL,
    [Film_Id] int  NOT NULL,
    [CustomProperty_Id] int  NOT NULL
);
GO

-- Creating table 'AdviceSet'
CREATE TABLE [dbo].[AdviceSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Key] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'AdviceCustomPropertySet'
CREATE TABLE [dbo].[AdviceCustomPropertySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] int  NOT NULL,
    [CustomProperty_Id] int  NOT NULL,
    [Advice_Id] int  NULL
);
GO

-- Creating table 'AdviceFilmSet'
CREATE TABLE [dbo].[AdviceFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] int  NOT NULL,
    [Advice_Id] int  NULL,
    [Film_Id] int  NOT NULL
);
GO

-- Creating table 'IMDbLoadingSet'
CREATE TABLE [dbo].[IMDbLoadingSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DataSetID] nvarchar(max)  NOT NULL,
    [IMDbId] nvarchar(max)  NULL,
    [EnglishTitle] nvarchar(max)  NULL,
    [Year] nvarchar(max)  NULL,
    [EnglishGenries] nvarchar(max)  NULL,
    [EnglishDescription] nvarchar(max)  NULL,
    [Poster] nvarchar(max)  NULL,
    [EnglishActors] nvarchar(max)  NULL,
    [EnglishCountries] nvarchar(max)  NULL,
    [EnglishProducers] nvarchar(max)  NULL,
    [Rating] nvarchar(max)  NULL,
    [EnglishTags] nvarchar(max)  NULL,
    [RussianTitle] nvarchar(max)  NULL,
    [RussianGenries] nvarchar(max)  NULL,
    [RussianDescription] nvarchar(max)  NULL,
    [RussianActors] nvarchar(max)  NULL,
    [RussianCountries] nvarchar(max)  NULL,
    [RussianProducers] nvarchar(max)  NULL,
    [RussianTags] nvarchar(max)  NULL
);
GO

-- Creating table 'PreprocessQuestionsSet'
CREATE TABLE [dbo].[PreprocessQuestionsSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [IKnowThatIWant] bit  NULL,
    [ActiveFilterType] int  NOT NULL,
    [Session_Id] int  NOT NULL
);
GO

-- Creating table 'GenreForFilterSet'
CREATE TABLE [dbo].[GenreForFilterSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PreprocessQuestions_Id] int  NOT NULL,
    [Genre_Id] int  NOT NULL
);
GO

-- Creating table 'CustomPropertyForFilterSet'
CREATE TABLE [dbo].[CustomPropertyForFilterSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PreprocessQuestions_Id] int  NOT NULL,
    [CustomProperty_Id] int  NOT NULL
);
GO

-- Creating table 'FinalSolutionSet'
CREATE TABLE [dbo].[FinalSolutionSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [VariableName] nvarchar(max)  NOT NULL,
    [Value] nvarchar(max)  NOT NULL,
    [Consultation_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'ExpertSystemSet'
ALTER TABLE [dbo].[ExpertSystemSet]
ADD CONSTRAINT [PK_ExpertSystemSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DomainSet'
ALTER TABLE [dbo].[DomainSet]
ADD CONSTRAINT [PK_DomainSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DomainValueSet'
ALTER TABLE [dbo].[DomainValueSet]
ADD CONSTRAINT [PK_DomainValueSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'VariableSet'
ALTER TABLE [dbo].[VariableSet]
ADD CONSTRAINT [PK_VariableSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FactSet'
ALTER TABLE [dbo].[FactSet]
ADD CONSTRAINT [PK_FactSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'RuleSet'
ALTER TABLE [dbo].[RuleSet]
ADD CONSTRAINT [PK_RuleSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'RuleFactSet'
ALTER TABLE [dbo].[RuleFactSet]
ADD CONSTRAINT [PK_RuleFactSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ConsultationSet'
ALTER TABLE [dbo].[ConsultationSet]
ADD CONSTRAINT [PK_ConsultationSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SessionSet'
ALTER TABLE [dbo].[SessionSet]
ADD CONSTRAINT [PK_SessionSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ConsultationFactSet'
ALTER TABLE [dbo].[ConsultationFactSet]
ADD CONSTRAINT [PK_ConsultationFactSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ConsultationRuleSet'
ALTER TABLE [dbo].[ConsultationRuleSet]
ADD CONSTRAINT [PK_ConsultationRuleSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GoalStackSet'
ALTER TABLE [dbo].[GoalStackSet]
ADD CONSTRAINT [PK_GoalStackSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ReviewSet'
ALTER TABLE [dbo].[ReviewSet]
ADD CONSTRAINT [PK_ReviewSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FilmSet'
ALTER TABLE [dbo].[FilmSet]
ADD CONSTRAINT [PK_FilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CountrySet'
ALTER TABLE [dbo].[CountrySet]
ADD CONSTRAINT [PK_CountrySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CountryFilmSet'
ALTER TABLE [dbo].[CountryFilmSet]
ADD CONSTRAINT [PK_CountryFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ActorFilmSet'
ALTER TABLE [dbo].[ActorFilmSet]
ADD CONSTRAINT [PK_ActorFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ActorSet'
ALTER TABLE [dbo].[ActorSet]
ADD CONSTRAINT [PK_ActorSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProducerSet'
ALTER TABLE [dbo].[ProducerSet]
ADD CONSTRAINT [PK_ProducerSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GenreFilmSet'
ALTER TABLE [dbo].[GenreFilmSet]
ADD CONSTRAINT [PK_GenreFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GenreSet'
ALTER TABLE [dbo].[GenreSet]
ADD CONSTRAINT [PK_GenreSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProducerFilmSet'
ALTER TABLE [dbo].[ProducerFilmSet]
ADD CONSTRAINT [PK_ProducerFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CustomPropertySet'
ALTER TABLE [dbo].[CustomPropertySet]
ADD CONSTRAINT [PK_CustomPropertySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FilmCustomPropertySet'
ALTER TABLE [dbo].[FilmCustomPropertySet]
ADD CONSTRAINT [PK_FilmCustomPropertySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'AdviceSet'
ALTER TABLE [dbo].[AdviceSet]
ADD CONSTRAINT [PK_AdviceSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'AdviceCustomPropertySet'
ALTER TABLE [dbo].[AdviceCustomPropertySet]
ADD CONSTRAINT [PK_AdviceCustomPropertySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'AdviceFilmSet'
ALTER TABLE [dbo].[AdviceFilmSet]
ADD CONSTRAINT [PK_AdviceFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'IMDbLoadingSet'
ALTER TABLE [dbo].[IMDbLoadingSet]
ADD CONSTRAINT [PK_IMDbLoadingSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'PreprocessQuestionsSet'
ALTER TABLE [dbo].[PreprocessQuestionsSet]
ADD CONSTRAINT [PK_PreprocessQuestionsSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GenreForFilterSet'
ALTER TABLE [dbo].[GenreForFilterSet]
ADD CONSTRAINT [PK_GenreForFilterSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CustomPropertyForFilterSet'
ALTER TABLE [dbo].[CustomPropertyForFilterSet]
ADD CONSTRAINT [PK_CustomPropertyForFilterSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FinalSolutionSet'
ALTER TABLE [dbo].[FinalSolutionSet]
ADD CONSTRAINT [PK_FinalSolutionSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ExpertSystemDomain_Domain_Id] in table 'DomainSet'
ALTER TABLE [dbo].[DomainSet]
ADD CONSTRAINT [FK_ExpertSystemDomain]
    FOREIGN KEY ([ExpertSystemDomain_Domain_Id])
    REFERENCES [dbo].[ExpertSystemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExpertSystemDomain'
CREATE INDEX [IX_FK_ExpertSystemDomain]
ON [dbo].[DomainSet]
    ([ExpertSystemDomain_Domain_Id]);
GO

-- Creating foreign key on [DomainDomainValue_DomainValue_Id] in table 'DomainValueSet'
ALTER TABLE [dbo].[DomainValueSet]
ADD CONSTRAINT [FK_DomainDomainValue]
    FOREIGN KEY ([DomainDomainValue_DomainValue_Id])
    REFERENCES [dbo].[DomainSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DomainDomainValue'
CREATE INDEX [IX_FK_DomainDomainValue]
ON [dbo].[DomainValueSet]
    ([DomainDomainValue_DomainValue_Id]);
GO

-- Creating foreign key on [Domain_Id] in table 'VariableSet'
ALTER TABLE [dbo].[VariableSet]
ADD CONSTRAINT [FK_DomainVariable]
    FOREIGN KEY ([Domain_Id])
    REFERENCES [dbo].[DomainSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DomainVariable'
CREATE INDEX [IX_FK_DomainVariable]
ON [dbo].[VariableSet]
    ([Domain_Id]);
GO

-- Creating foreign key on [ExpertSystemVariable_Variable_Id] in table 'VariableSet'
ALTER TABLE [dbo].[VariableSet]
ADD CONSTRAINT [FK_ExpertSystemVariable]
    FOREIGN KEY ([ExpertSystemVariable_Variable_Id])
    REFERENCES [dbo].[ExpertSystemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExpertSystemVariable'
CREATE INDEX [IX_FK_ExpertSystemVariable]
ON [dbo].[VariableSet]
    ([ExpertSystemVariable_Variable_Id]);
GO

-- Creating foreign key on [ExpertSystemVariable1_Variable_Id] in table 'VariableSet'
ALTER TABLE [dbo].[VariableSet]
ADD CONSTRAINT [FK_ExpertSystemVariable1]
    FOREIGN KEY ([ExpertSystemVariable1_Variable_Id])
    REFERENCES [dbo].[ExpertSystemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExpertSystemVariable1'
CREATE INDEX [IX_FK_ExpertSystemVariable1]
ON [dbo].[VariableSet]
    ([ExpertSystemVariable1_Variable_Id]);
GO

-- Creating foreign key on [DomainValue_Id] in table 'FactSet'
ALTER TABLE [dbo].[FactSet]
ADD CONSTRAINT [FK_DomainValueFact]
    FOREIGN KEY ([DomainValue_Id])
    REFERENCES [dbo].[DomainValueSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_DomainValueFact'
CREATE INDEX [IX_FK_DomainValueFact]
ON [dbo].[FactSet]
    ([DomainValue_Id]);
GO

-- Creating foreign key on [Variable_Id] in table 'FactSet'
ALTER TABLE [dbo].[FactSet]
ADD CONSTRAINT [FK_VariableFact]
    FOREIGN KEY ([Variable_Id])
    REFERENCES [dbo].[VariableSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_VariableFact'
CREATE INDEX [IX_FK_VariableFact]
ON [dbo].[FactSet]
    ([Variable_Id]);
GO

-- Creating foreign key on [Result_Id] in table 'RuleSet'
ALTER TABLE [dbo].[RuleSet]
ADD CONSTRAINT [FK_FactRule]
    FOREIGN KEY ([Result_Id])
    REFERENCES [dbo].[FactSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FactRule'
CREATE INDEX [IX_FK_FactRule]
ON [dbo].[RuleSet]
    ([Result_Id]);
GO

-- Creating foreign key on [Rule_Id] in table 'RuleFactSet'
ALTER TABLE [dbo].[RuleFactSet]
ADD CONSTRAINT [FK_RuleRuleFact]
    FOREIGN KEY ([Rule_Id])
    REFERENCES [dbo].[RuleSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RuleRuleFact'
CREATE INDEX [IX_FK_RuleRuleFact]
ON [dbo].[RuleFactSet]
    ([Rule_Id]);
GO

-- Creating foreign key on [Fact_Id] in table 'RuleFactSet'
ALTER TABLE [dbo].[RuleFactSet]
ADD CONSTRAINT [FK_FactRuleFact]
    FOREIGN KEY ([Fact_Id])
    REFERENCES [dbo].[FactSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FactRuleFact'
CREATE INDEX [IX_FK_FactRuleFact]
ON [dbo].[RuleFactSet]
    ([Fact_Id]);
GO

-- Creating foreign key on [ExpertSystemRule_Rule_Id] in table 'RuleSet'
ALTER TABLE [dbo].[RuleSet]
ADD CONSTRAINT [FK_ExpertSystemRule]
    FOREIGN KEY ([ExpertSystemRule_Rule_Id])
    REFERENCES [dbo].[ExpertSystemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExpertSystemRule'
CREATE INDEX [IX_FK_ExpertSystemRule]
ON [dbo].[RuleSet]
    ([ExpertSystemRule_Rule_Id]);
GO

-- Creating foreign key on [Session_Id] in table 'ConsultationSet'
ALTER TABLE [dbo].[ConsultationSet]
ADD CONSTRAINT [FK_SessionConsultation]
    FOREIGN KEY ([Session_Id])
    REFERENCES [dbo].[SessionSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SessionConsultation'
CREATE INDEX [IX_FK_SessionConsultation]
ON [dbo].[ConsultationSet]
    ([Session_Id]);
GO

-- Creating foreign key on [ExpertSystem_Id] in table 'ConsultationSet'
ALTER TABLE [dbo].[ConsultationSet]
ADD CONSTRAINT [FK_ExpertSystemConsultation]
    FOREIGN KEY ([ExpertSystem_Id])
    REFERENCES [dbo].[ExpertSystemSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExpertSystemConsultation'
CREATE INDEX [IX_FK_ExpertSystemConsultation]
ON [dbo].[ConsultationSet]
    ([ExpertSystem_Id]);
GO

-- Creating foreign key on [Consultation_Id] in table 'ConsultationFactSet'
ALTER TABLE [dbo].[ConsultationFactSet]
ADD CONSTRAINT [FK_ConsultationConsultationFact]
    FOREIGN KEY ([Consultation_Id])
    REFERENCES [dbo].[ConsultationSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ConsultationConsultationFact'
CREATE INDEX [IX_FK_ConsultationConsultationFact]
ON [dbo].[ConsultationFactSet]
    ([Consultation_Id]);
GO

-- Creating foreign key on [Consultation_Id] in table 'ConsultationRuleSet'
ALTER TABLE [dbo].[ConsultationRuleSet]
ADD CONSTRAINT [FK_ConsultationConsultationRule]
    FOREIGN KEY ([Consultation_Id])
    REFERENCES [dbo].[ConsultationSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ConsultationConsultationRule'
CREATE INDEX [IX_FK_ConsultationConsultationRule]
ON [dbo].[ConsultationRuleSet]
    ([Consultation_Id]);
GO

-- Creating foreign key on [Fact_Id] in table 'ConsultationFactSet'
ALTER TABLE [dbo].[ConsultationFactSet]
ADD CONSTRAINT [FK_FactConsultationFact]
    FOREIGN KEY ([Fact_Id])
    REFERENCES [dbo].[FactSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FactConsultationFact'
CREATE INDEX [IX_FK_FactConsultationFact]
ON [dbo].[ConsultationFactSet]
    ([Fact_Id]);
GO

-- Creating foreign key on [Rule_Id] in table 'ConsultationRuleSet'
ALTER TABLE [dbo].[ConsultationRuleSet]
ADD CONSTRAINT [FK_RuleConsultationRule]
    FOREIGN KEY ([Rule_Id])
    REFERENCES [dbo].[RuleSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RuleConsultationRule'
CREATE INDEX [IX_FK_RuleConsultationRule]
ON [dbo].[ConsultationRuleSet]
    ([Rule_Id]);
GO

-- Creating foreign key on [CurrentRule_Id] in table 'ConsultationSet'
ALTER TABLE [dbo].[ConsultationSet]
ADD CONSTRAINT [FK_RuleConsultation]
    FOREIGN KEY ([CurrentRule_Id])
    REFERENCES [dbo].[RuleSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RuleConsultation'
CREATE INDEX [IX_FK_RuleConsultation]
ON [dbo].[ConsultationSet]
    ([CurrentRule_Id]);
GO

-- Creating foreign key on [GoalStack_Id] in table 'ConsultationSet'
ALTER TABLE [dbo].[ConsultationSet]
ADD CONSTRAINT [FK_GoalStackConsultation]
    FOREIGN KEY ([GoalStack_Id])
    REFERENCES [dbo].[GoalStackSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GoalStackConsultation'
CREATE INDEX [IX_FK_GoalStackConsultation]
ON [dbo].[ConsultationSet]
    ([GoalStack_Id]);
GO

-- Creating foreign key on [Variable_Id] in table 'GoalStackSet'
ALTER TABLE [dbo].[GoalStackSet]
ADD CONSTRAINT [FK_VariableGoalStack]
    FOREIGN KEY ([Variable_Id])
    REFERENCES [dbo].[VariableSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_VariableGoalStack'
CREATE INDEX [IX_FK_VariableGoalStack]
ON [dbo].[GoalStackSet]
    ([Variable_Id]);
GO

-- Creating foreign key on [NextGoal_Id] in table 'GoalStackSet'
ALTER TABLE [dbo].[GoalStackSet]
ADD CONSTRAINT [FK_GoalStackGoalStack]
    FOREIGN KEY ([NextGoal_Id])
    REFERENCES [dbo].[GoalStackSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GoalStackGoalStack'
CREATE INDEX [IX_FK_GoalStackGoalStack]
ON [dbo].[GoalStackSet]
    ([NextGoal_Id]);
GO

-- Creating foreign key on [Session_Id] in table 'ReviewSet'
ALTER TABLE [dbo].[ReviewSet]
ADD CONSTRAINT [FK_SessionReview]
    FOREIGN KEY ([Session_Id])
    REFERENCES [dbo].[SessionSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SessionReview'
CREATE INDEX [IX_FK_SessionReview]
ON [dbo].[ReviewSet]
    ([Session_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'CountryFilmSet'
ALTER TABLE [dbo].[CountryFilmSet]
ADD CONSTRAINT [FK_FilmCountryFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmCountryFilm'
CREATE INDEX [IX_FK_FilmCountryFilm]
ON [dbo].[CountryFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'ActorFilmSet'
ALTER TABLE [dbo].[ActorFilmSet]
ADD CONSTRAINT [FK_FilmActorFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmActorFilm'
CREATE INDEX [IX_FK_FilmActorFilm]
ON [dbo].[ActorFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'GenreFilmSet'
ALTER TABLE [dbo].[GenreFilmSet]
ADD CONSTRAINT [FK_FilmGenreFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmGenreFilm'
CREATE INDEX [IX_FK_FilmGenreFilm]
ON [dbo].[GenreFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Country_Id] in table 'CountryFilmSet'
ALTER TABLE [dbo].[CountryFilmSet]
ADD CONSTRAINT [FK_CountryCountryFilm]
    FOREIGN KEY ([Country_Id])
    REFERENCES [dbo].[CountrySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CountryCountryFilm'
CREATE INDEX [IX_FK_CountryCountryFilm]
ON [dbo].[CountryFilmSet]
    ([Country_Id]);
GO

-- Creating foreign key on [Actor_Id] in table 'ActorFilmSet'
ALTER TABLE [dbo].[ActorFilmSet]
ADD CONSTRAINT [FK_ActorActorFilm]
    FOREIGN KEY ([Actor_Id])
    REFERENCES [dbo].[ActorSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ActorActorFilm'
CREATE INDEX [IX_FK_ActorActorFilm]
ON [dbo].[ActorFilmSet]
    ([Actor_Id]);
GO

-- Creating foreign key on [Genre_Id] in table 'GenreFilmSet'
ALTER TABLE [dbo].[GenreFilmSet]
ADD CONSTRAINT [FK_GenreGenreFilm]
    FOREIGN KEY ([Genre_Id])
    REFERENCES [dbo].[GenreSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GenreGenreFilm'
CREATE INDEX [IX_FK_GenreGenreFilm]
ON [dbo].[GenreFilmSet]
    ([Genre_Id]);
GO

-- Creating foreign key on [Producer_Id] in table 'ProducerFilmSet'
ALTER TABLE [dbo].[ProducerFilmSet]
ADD CONSTRAINT [FK_ProducerProducerFilm]
    FOREIGN KEY ([Producer_Id])
    REFERENCES [dbo].[ProducerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ProducerProducerFilm'
CREATE INDEX [IX_FK_ProducerProducerFilm]
ON [dbo].[ProducerFilmSet]
    ([Producer_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'ProducerFilmSet'
ALTER TABLE [dbo].[ProducerFilmSet]
ADD CONSTRAINT [FK_FilmProducerFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmProducerFilm'
CREATE INDEX [IX_FK_FilmProducerFilm]
ON [dbo].[ProducerFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'FilmCustomPropertySet'
ALTER TABLE [dbo].[FilmCustomPropertySet]
ADD CONSTRAINT [FK_FilmFilmCustomProperty]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmFilmCustomProperty'
CREATE INDEX [IX_FK_FilmFilmCustomProperty]
ON [dbo].[FilmCustomPropertySet]
    ([Film_Id]);
GO

-- Creating foreign key on [CustomProperty_Id] in table 'FilmCustomPropertySet'
ALTER TABLE [dbo].[FilmCustomPropertySet]
ADD CONSTRAINT [FK_CustomPropertyFilmCustomProperty]
    FOREIGN KEY ([CustomProperty_Id])
    REFERENCES [dbo].[CustomPropertySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomPropertyFilmCustomProperty'
CREATE INDEX [IX_FK_CustomPropertyFilmCustomProperty]
ON [dbo].[FilmCustomPropertySet]
    ([CustomProperty_Id]);
GO

-- Creating foreign key on [CustomProperty_Id] in table 'AdviceCustomPropertySet'
ALTER TABLE [dbo].[AdviceCustomPropertySet]
ADD CONSTRAINT [FK_CustomPropertyAdviceCustomProperty]
    FOREIGN KEY ([CustomProperty_Id])
    REFERENCES [dbo].[CustomPropertySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomPropertyAdviceCustomProperty'
CREATE INDEX [IX_FK_CustomPropertyAdviceCustomProperty]
ON [dbo].[AdviceCustomPropertySet]
    ([CustomProperty_Id]);
GO

-- Creating foreign key on [Advice_Id] in table 'AdviceFilmSet'
ALTER TABLE [dbo].[AdviceFilmSet]
ADD CONSTRAINT [FK_AdviceAdviceFilm]
    FOREIGN KEY ([Advice_Id])
    REFERENCES [dbo].[AdviceSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AdviceAdviceFilm'
CREATE INDEX [IX_FK_AdviceAdviceFilm]
ON [dbo].[AdviceFilmSet]
    ([Advice_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'AdviceFilmSet'
ALTER TABLE [dbo].[AdviceFilmSet]
ADD CONSTRAINT [FK_FilmAdviceFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmAdviceFilm'
CREATE INDEX [IX_FK_FilmAdviceFilm]
ON [dbo].[AdviceFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Advice_Id] in table 'AdviceCustomPropertySet'
ALTER TABLE [dbo].[AdviceCustomPropertySet]
ADD CONSTRAINT [FK_AdviceAdviceCustomProperty]
    FOREIGN KEY ([Advice_Id])
    REFERENCES [dbo].[AdviceSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AdviceAdviceCustomProperty'
CREATE INDEX [IX_FK_AdviceAdviceCustomProperty]
ON [dbo].[AdviceCustomPropertySet]
    ([Advice_Id]);
GO

-- Creating foreign key on [Session_Id] in table 'PreprocessQuestionsSet'
ALTER TABLE [dbo].[PreprocessQuestionsSet]
ADD CONSTRAINT [FK_PreprocessQuestionsSession]
    FOREIGN KEY ([Session_Id])
    REFERENCES [dbo].[SessionSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PreprocessQuestionsSession'
CREATE INDEX [IX_FK_PreprocessQuestionsSession]
ON [dbo].[PreprocessQuestionsSet]
    ([Session_Id]);
GO

-- Creating foreign key on [PreprocessQuestions_Id] in table 'GenreForFilterSet'
ALTER TABLE [dbo].[GenreForFilterSet]
ADD CONSTRAINT [FK_PreprocessQuestionsGenreForFilter]
    FOREIGN KEY ([PreprocessQuestions_Id])
    REFERENCES [dbo].[PreprocessQuestionsSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PreprocessQuestionsGenreForFilter'
CREATE INDEX [IX_FK_PreprocessQuestionsGenreForFilter]
ON [dbo].[GenreForFilterSet]
    ([PreprocessQuestions_Id]);
GO

-- Creating foreign key on [Genre_Id] in table 'GenreForFilterSet'
ALTER TABLE [dbo].[GenreForFilterSet]
ADD CONSTRAINT [FK_GenreGenreForFilter]
    FOREIGN KEY ([Genre_Id])
    REFERENCES [dbo].[GenreSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GenreGenreForFilter'
CREATE INDEX [IX_FK_GenreGenreForFilter]
ON [dbo].[GenreForFilterSet]
    ([Genre_Id]);
GO

-- Creating foreign key on [PreprocessQuestions_Id] in table 'CustomPropertyForFilterSet'
ALTER TABLE [dbo].[CustomPropertyForFilterSet]
ADD CONSTRAINT [FK_PreprocessQuestionsCustomPropertyForFilter]
    FOREIGN KEY ([PreprocessQuestions_Id])
    REFERENCES [dbo].[PreprocessQuestionsSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PreprocessQuestionsCustomPropertyForFilter'
CREATE INDEX [IX_FK_PreprocessQuestionsCustomPropertyForFilter]
ON [dbo].[CustomPropertyForFilterSet]
    ([PreprocessQuestions_Id]);
GO

-- Creating foreign key on [CustomProperty_Id] in table 'CustomPropertyForFilterSet'
ALTER TABLE [dbo].[CustomPropertyForFilterSet]
ADD CONSTRAINT [FK_CustomPropertyCustomPropertyForFilter]
    FOREIGN KEY ([CustomProperty_Id])
    REFERENCES [dbo].[CustomPropertySet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CustomPropertyCustomPropertyForFilter'
CREATE INDEX [IX_FK_CustomPropertyCustomPropertyForFilter]
ON [dbo].[CustomPropertyForFilterSet]
    ([CustomProperty_Id]);
GO

-- Creating foreign key on [Consultation_Id] in table 'FinalSolutionSet'
ALTER TABLE [dbo].[FinalSolutionSet]
ADD CONSTRAINT [FK_FinalSolutionConsultation]
    FOREIGN KEY ([Consultation_Id])
    REFERENCES [dbo].[ConsultationSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FinalSolutionConsultation'
CREATE INDEX [IX_FK_FinalSolutionConsultation]
ON [dbo].[FinalSolutionSet]
    ([Consultation_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------