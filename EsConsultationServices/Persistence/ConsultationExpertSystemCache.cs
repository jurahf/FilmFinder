using System.Collections.Concurrent;
using EsConsultationServices.Contracts;
using EsConsultationServices.Engine;
using EsDb.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace EsConsultationServices.Persistence
{
    /// <summary>
    /// Кэш неизменяемого графа ЭС. Один граф используется всеми сессиями и потоками.
    /// Загрузчик singleton, при промахе создаёт scope для scoped EsDbContext/провайдеров.
    /// </summary>
    public sealed class ConsultationExpertSystemCache
    {
        private static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(1);
        private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromHours(24);

        private readonly IMemoryCache cache;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ConcurrentDictionary<int, byte> knownKeys = new();

        public ConsultationExpertSystemCache(IMemoryCache cache, IServiceScopeFactory scopeFactory)
        {
            this.cache = cache;
            this.scopeFactory = scopeFactory;
        }

        public async Task<ConsultationExpertSystem> GetAsync(int expertSystemId, CancellationToken cancellationToken = default)
        {
            var key = GetKey(expertSystemId);
            if (cache.TryGetValue(key, out ConsultationExpertSystem? cached) && cached != null)
                return cached;

            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IExpertSystemRepository>();
            var displayProvider = scope.ServiceProvider.GetService<IConsultationDisplayProvider>();

            var displays = displayProvider == null
                ? Array.Empty<ConsultationDisplayValue>()
                : await displayProvider.GetDisplayValuesAsync(cancellationToken);

            var expertSystem = await LoadAsync(expertSystemId, repository, displays, cancellationToken);

            knownKeys[expertSystemId] = 0;
            cache.Set(key, expertSystem, new MemoryCacheEntryOptions
            {
                SlidingExpiration = SlidingExpiration,
                AbsoluteExpirationRelativeToNow = AbsoluteExpiration
            });

            return expertSystem;
        }

        public void Invalidate(int expertSystemId)
        {
            knownKeys.TryRemove(expertSystemId, out _);
            cache.Remove(GetKey(expertSystemId));
        }

        public void InvalidateAll()
        {
            foreach (var expertSystemId in knownKeys.Keys)
                cache.Remove(GetKey(expertSystemId));

            knownKeys.Clear();
        }

        private static async Task<ConsultationExpertSystem> LoadAsync(
            int expertSystemId,
            IExpertSystemRepository repository,
            IReadOnlyList<ConsultationDisplayValue> displays,
            CancellationToken cancellationToken)
        {
            var database = await repository.GetGraphAsync(expertSystemId);
            if (database == null)
                throw new KeyNotFoundException($"Экспертная система с Id={expertSystemId} не найдена.");

            var facts = await repository.GetFactsAsync(expertSystemId);
            var displayLookup = BuildDisplayLookup(displays);

            var domains = new Dictionary<int, ConsultationDomain>();
            var domainValues = new Dictionary<int, ConsultationDomainValue>();

            foreach (var dbDomain in database.AllDomains.OrderBy(x => x.Id))
            {
                var values = dbDomain.DomainValue
                    .OrderBy(x => x.Id)
                    .Select(dbValue => new ConsultationDomainValue
                    {
                        Id = dbValue.Id,
                        Value = dbValue.Value,
                        Display = ResolveDisplay(displayLookup, dbDomain.Name, dbValue.Value)
                    })
                    .ToList();

                var domain = new ConsultationDomain
                {
                    Id = dbDomain.Id,
                    Name = dbDomain.Name,
                    Values = values
                };

                domains[dbDomain.Id] = domain;

                foreach (var value in values)
                    domainValues[value.Id] = value;
            }

            var variables = new Dictionary<int, ConsultationVariable>();
            foreach (var dbVariable in database.AllVariables.OrderBy(x => x.Id))
            {
                var variable = new ConsultationVariable
                {
                    Id = dbVariable.Id,
                    Name = dbVariable.Name,
                    Question = dbVariable.Question,
                    Reasoning = dbVariable.Reasoning,
                    Type = dbVariable.Type,
                    Domain = dbVariable.DomainId.HasValue && domains.TryGetValue(dbVariable.DomainId.Value, out var domain)
                        ? domain
                        : null
                };

                variables[dbVariable.Id] = variable;
            }

            var factModels = new Dictionary<int, ConsultationFact>();
            var allFacts = new List<ConsultationFact>();

            foreach (var dbFact in facts.OrderBy(x => x.Id))
            {
                if (!variables.TryGetValue(dbFact.VariableId, out var variable))
                    continue;

                if (!dbFact.DomainValueId.HasValue ||
                    !domainValues.TryGetValue(dbFact.DomainValueId.Value, out var domainValue))
                {
                    continue;
                }

                var fact = new ConsultationFact
                {
                    Id = dbFact.Id,
                    Variable = variable,
                    DomainValue = domainValue
                };

                factModels[fact.Id] = fact;
                allFacts.Add(fact);
            }

            var rules = new List<ConsultationRule>();
            foreach (var dbRule in database.AllRules.OrderBy(x => x.Order).ThenBy(x => x.Id))
            {
                var conditions = dbRule.Conditions
                    .OrderBy(x => x.Order ?? int.MaxValue)
                    .ThenBy(x => x.Id)
                    .Where(x => factModels.ContainsKey(x.FactId))
                    .Select(x => new ConsultationRuleFact
                    {
                        Order = x.Order ?? 0,
                        Fact = factModels[x.FactId]
                    })
                    .ToList();

                ConsultationFact? result = null;
                if (dbRule.ResultId.HasValue && factModels.TryGetValue(dbRule.ResultId.Value, out var resultFact))
                    result = resultFact;

                rules.Add(new ConsultationRule
                {
                    Id = dbRule.Id,
                    Order = dbRule.Order,
                    Name = dbRule.Name,
                    Reasoning = dbRule.Reasoning,
                    Result = result,
                    Conditions = conditions
                });
            }

            var target = database.TargetId.HasValue && variables.TryGetValue(database.TargetId.Value, out var targetVariable)
                ? targetVariable
                : null;

            return new ConsultationExpertSystem
            {
                Id = database.Id,
                Name = database.Name,
                Target = target,
                Variables = variables.Values.OrderBy(x => x.Id).ToList(),
                Rules = rules,
                Facts = allFacts
            };
        }

        private static Dictionary<string, string> BuildDisplayLookup(
            IReadOnlyList<ConsultationDisplayValue> displays)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var display in displays)
            {
                result[BuildDisplayKey(display.DomainName, display.Value)] = display.Display;
            }

            return result;
        }

        private static string ResolveDisplay(
            IReadOnlyDictionary<string, string> displayLookup,
            string domainName,
            string value)
        {
            return displayLookup.TryGetValue(BuildDisplayKey(domainName, value), out var display)
                ? display
                : value;
        }

        private static string BuildDisplayKey(string domainName, string value)
            => $"{domainName}\u001f{value}";

        private static string GetKey(int expertSystemId) => $"consultation-es:{expertSystemId}";
    }
}
