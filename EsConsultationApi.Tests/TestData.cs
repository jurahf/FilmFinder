using EsConsultationServices.Films;
using EsDb;
using EsDb.Model;
using EsDb.Model.Enums;
using FilmDb;
using FilmDb.Model;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationApi.Tests;

internal static class TestData
{
    public static readonly Guid AdviceKey = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Создаёт ЭС «Фильмы»: вопрос Q (Да/Нет), цель «Совет» с доменом #Советы,
    /// правило Q = Да -> Совет = GUID совета.
    /// </summary>
    public static async Task<int> SeedExpertSystemAsync(EsDbContext db)
    {
        var yes = new DomainValue { Value = "Да" };
        var no = new DomainValue { Value = "Нет" };
        var questionDomain = new Domain { Name = "ДаНет", DomainValue = { yes, no } };

        var adviceValue = new DomainValue { Value = AdviceKey.ToString("D") };
        var adviceDomain = new Domain
        {
            Name = AdviceCatalogConstants.DomainName,
            DomainValue = { adviceValue }
        };

        var question = new Variable
        {
            Name = "Q",
            Question = "Вопрос?",
            Type = VariableType.Queriable,
            Domain = questionDomain
        };

        var target = new Variable
        {
            Name = "Совет",
            Type = VariableType.Detective,
            Domain = adviceDomain
        };

        var condition = new Fact { Variable = question, DomainValue = yes };
        var result = new Fact { Variable = target, DomainValue = adviceValue };

        var rule = new Rule
        {
            Order = 0,
            Name = "R",
            Result = result,
            Conditions = { new RuleFact { Order = 0, Fact = condition } }
        };

        var expertSystem = new ExpertSystem
        {
            Name = "Фильмы",
            AllDomains = { questionDomain, adviceDomain },
            AllVariables = { question, target },
            AllRules = { rule }
        };

        questionDomain.ExpertSystem = expertSystem;
        adviceDomain.ExpertSystem = expertSystem;
        question.ExpertSystem = expertSystem;
        target.ExpertSystem = expertSystem;
        rule.ExpertSystem = expertSystem;

        db.ExpertSystems.Add(expertSystem);
        await db.SaveChangesAsync();

        expertSystem.TargetId = target.Id;
        await db.SaveChangesAsync();

        return expertSystem.Id;
    }

    /// <summary>
    /// Совет с явными фильмами A (100) и B (90) и свойствами Смешное = 80, Мрачное = -50.
    /// Фильмы C, D, E похожи по свойствам и используются как fallback.
    /// </summary>
    public static async Task<FilmSeed> SeedFilmsAsync(FilmDbContext db, bool withProperties = true)
    {
        var funny = new CustomProperty { Name = "Смешное" };
        var dark = new CustomProperty { Name = "Мрачное" };

        var filmA = new Film { Name = "Фильм A", Year = 2001, Rating = 8.5m };
        var filmB = new Film { Name = "Фильм B", Year = 2002, Rating = 8.0m };
        var filmC = new Film { Name = "Фильм C", Year = 2003, Rating = 7.0m };
        var filmD = new Film { Name = "Фильм D", Year = 2004, Rating = 6.5m };
        var filmE = new Film { Name = "Фильм E", Year = 2005, Rating = 6.0m };

        db.CustomProperties.AddRange(funny, dark);
        db.Films.AddRange(filmA, filmB, filmC, filmD, filmE);
        await db.SaveChangesAsync();

        var advice = new Advice { Key = AdviceKey };
        advice.AdviceFilm.Add(new AdviceFilm { FilmId = filmA.Id, Value = 100 });
        advice.AdviceFilm.Add(new AdviceFilm { FilmId = filmB.Id, Value = 90 });

        if (withProperties)
        {
            advice.AdviceCustomProperty.Add(new AdviceCustomProperty
            {
                CustomPropertyId = funny.Id,
                Value = 80
            });
            advice.AdviceCustomProperty.Add(new AdviceCustomProperty
            {
                CustomPropertyId = dark.Id,
                Value = -50
            });
        }

        db.Advices.Add(advice);

        db.FilmCustomProperties.AddRange(
            new FilmCustomProperty { Film = filmC, CustomProperty = funny, Value = 85 },
            new FilmCustomProperty { Film = filmD, CustomProperty = funny, Value = 78 },
            new FilmCustomProperty { Film = filmE, CustomProperty = funny, Value = 40 });

        await db.SaveChangesAsync();

        return new FilmSeed(filmA.Id, filmB.Id, filmC.Id, filmD.Id, filmE.Id);
    }
}

internal sealed record FilmSeed(int A, int B, int C, int D, int E)
{
    public IReadOnlyList<int> Fallback => new[] { C, D, E };
}
