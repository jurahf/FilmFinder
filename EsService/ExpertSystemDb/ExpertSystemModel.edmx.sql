
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/01/2019 22:25:24
-- Generated from EDMX file: D:\PROJECTS\CSharp\FilmsFinder\EsService\ExpertSystemDb\ExpertSystemModel.edmx
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
    [Rule_Id] int  NOT NULL,
    [Fact_Id] int  NOT NULL
);
GO

-- Creating table 'ConsultationSet'
CREATE TABLE [dbo].[ConsultationSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
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
    [LastActivityDate] datetime  NULL,
    [SessionConsultation_Session_Id] int  NOT NULL
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

-- Creating foreign key on [SessionConsultation_Session_Id] in table 'SessionSet'
ALTER TABLE [dbo].[SessionSet]
ADD CONSTRAINT [FK_SessionConsultation]
    FOREIGN KEY ([SessionConsultation_Session_Id])
    REFERENCES [dbo].[ConsultationSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SessionConsultation'
CREATE INDEX [IX_FK_SessionConsultation]
ON [dbo].[SessionSet]
    ([SessionConsultation_Session_Id]);
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

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------