/*
    Перенос настроек ЭС из старой базы ExpertSystemDB в новую EsDb.

    Что переносится:
      ExpertSystemSet  -> ExpertSystems
      DomainSet        -> Domains
      DomainValueSet   -> DomainValues
      VariableSet      -> Variables
      FactSet          -> Facts
      RuleSet          -> Rules
      RuleFactSet      -> RuleFacts

    Что НЕ переносится:
      консультации, сессии, стеки целей и итоговые решения.
      Они остаются в ExpertSystemDB как архив.

    Скрипт рассчитан на однократный запуск в пустую EsDb.
    Перед запуском примените EF-миграцию EsDb (dotnet ef database update).
*/

USE [EsDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM dbo.ExpertSystems)
BEGIN
    ROLLBACK TRANSACTION;
    ;THROW 50000, N'База EsDb уже содержит экспертные системы. Импорт отменён.', 1;
END;

/* 1. Экспертные системы (цель заполним после переменных) */
SET IDENTITY_INSERT dbo.ExpertSystems ON;

INSERT INTO dbo.ExpertSystems (Id, Name, TargetId)
SELECT es.Id, es.Name, NULL
FROM ExpertSystemDB.dbo.ExpertSystemSet AS es;

SET IDENTITY_INSERT dbo.ExpertSystems OFF;

/* 2. Домены */
SET IDENTITY_INSERT dbo.Domains ON;

INSERT INTO dbo.Domains (Id, Name, ExpertSystemId)
SELECT d.Id, d.Name, d.ExpertSystemDomain_Domain_Id
FROM ExpertSystemDB.dbo.DomainSet AS d;

SET IDENTITY_INSERT dbo.Domains OFF;

/* 3. Значения доменов */
SET IDENTITY_INSERT dbo.DomainValues ON;

INSERT INTO dbo.DomainValues (Id, Value, DomainId)
SELECT dv.Id, dv.Value, dv.DomainDomainValue_DomainValue_Id
FROM ExpertSystemDB.dbo.DomainValueSet AS dv;

SET IDENTITY_INSERT dbo.DomainValues OFF;

/* 4. Переменные */
SET IDENTITY_INSERT dbo.Variables ON;

INSERT INTO dbo.Variables (Id, Name, Question, Reasoning, Type, DomainId, ExpertSystemId)
SELECT v.Id, v.Name, v.Question, v.Reasoning, v.Type, v.Domain_Id, v.ExpertSystemVariable_Variable_Id
FROM ExpertSystemDB.dbo.VariableSet AS v;

SET IDENTITY_INSERT dbo.Variables OFF;

/* 5. Цель консультации: в старой схеме это ExpertSystemVariable1_Variable_Id */
UPDATE es
SET es.TargetId = v.Id
FROM dbo.ExpertSystems AS es
JOIN ExpertSystemDB.dbo.VariableSet AS v
    ON v.ExpertSystemVariable1_Variable_Id = es.Id;

/* 6. Факты */
SET IDENTITY_INSERT dbo.Facts ON;

INSERT INTO dbo.Facts (Id, VariableId, DomainValueId)
SELECT f.Id, f.Variable_Id, f.DomainValue_Id
FROM ExpertSystemDB.dbo.FactSet AS f;

SET IDENTITY_INSERT dbo.Facts OFF;

/* 7. Правила */
SET IDENTITY_INSERT dbo.Rules ON;

INSERT INTO dbo.Rules (Id, [Order], Name, Reasoning, ResultId, ExpertSystemId)
SELECT r.Id, r.[Order], r.Name, r.Reasoning, r.Result_Id, r.ExpertSystemRule_Rule_Id
FROM ExpertSystemDB.dbo.RuleSet AS r;

SET IDENTITY_INSERT dbo.Rules OFF;

/* 8. Посылки правил */
SET IDENTITY_INSERT dbo.RuleFacts ON;

INSERT INTO dbo.RuleFacts (Id, [Order], RuleId, FactId)
SELECT rf.Id, rf.[Order], rf.Rule_Id, rf.Fact_Id
FROM ExpertSystemDB.dbo.RuleFactSet AS rf;

SET IDENTITY_INSERT dbo.RuleFacts OFF;

/* 9. Синхронизируем IDENTITY-счётчики */
DBCC CHECKIDENT ('dbo.ExpertSystems', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Domains', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.DomainValues', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Variables', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Facts', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Rules', RESEED) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.RuleFacts', RESEED) WITH NO_INFOMSGS;

COMMIT TRANSACTION;
GO

/* Контрольные суммы */
SELECT 'ExpertSystems' AS TableName, COUNT(*) AS [Rows] FROM dbo.ExpertSystems
UNION ALL SELECT 'Domains', COUNT(*) FROM dbo.Domains
UNION ALL SELECT 'DomainValues', COUNT(*) FROM dbo.DomainValues
UNION ALL SELECT 'Variables', COUNT(*) FROM dbo.Variables
UNION ALL SELECT 'Facts', COUNT(*) FROM dbo.Facts
UNION ALL SELECT 'Rules', COUNT(*) FROM dbo.Rules
UNION ALL SELECT 'RuleFacts', COUNT(*) FROM dbo.RuleFacts;
GO
