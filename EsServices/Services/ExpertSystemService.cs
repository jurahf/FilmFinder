using EsDb;
using EsDb.Model;
using EsDb.Repositories;
using EsServices.Converters;
using EsServices.Services.Advice;
using EsServices.Validators;
using EsServices.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace EsServices.Services
{
    public class ExpertSystemService : IExpertSystemService
    {
        private readonly EsDbContext db;
        private readonly IExpertSystemRepository repository;
        private readonly ExpertSystemConverter converter;
        private readonly ExpertSystemValidator validator;
        private readonly IAdviceCatalogService adviceCatalog;

        public ExpertSystemService(
            EsDbContext db,
            IExpertSystemRepository repository,
            ExpertSystemConverter converter,
            ExpertSystemValidator validator,
            IAdviceCatalogService adviceCatalog)
        {
            this.db = db;
            this.repository = repository;
            this.converter = converter;
            this.validator = validator;
            this.adviceCatalog = adviceCatalog;
        }

        public async Task<List<ExpertSystemSummaryVM>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var rows = await repository.GetListAsync();
            return rows
                .Select(x => new ExpertSystemSummaryVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    DomainCount = x.DomainCount,
                    VariableCount = x.VariableCount,
                    RuleCount = x.RuleCount
                })
                .ToList();
        }

        public async Task<ExpertSystemVM> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var database = await repository.GetGraphAsync(id);
            if (database == null)
                throw new KeyNotFoundException($"Экспертная система с Id={id} не найдена.");

            var facts = await repository.GetFactsAsync(id);
            var descriptions = await adviceCatalog.GetDescriptionsAsync(cancellationToken);

            return converter.ConvertToVm(database, facts, descriptions);
        }

        public async Task<ExpertSystemVM> CreateAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя экспертной системы не может быть пустым.", nameof(name));

            var normalizedName = name.Trim();
            if (await db.ExpertSystems.AnyAsync(x => x.Name == normalizedName, cancellationToken))
                throw new InvalidOperationException($"Экспертная система с именем '{normalizedName}' уже существует.");

            var expertSystem = new ExpertSystem
            {
                Name = normalizedName
            };

            db.ExpertSystems.Add(expertSystem);
            await db.SaveChangesAsync(cancellationToken);

            return await GetAsync(expertSystem.Id, cancellationToken);
        }

        public async Task<SaveExpertSystemResult> SaveAsNewVersionAsync(
            ExpertSystemVM expertSystem,
            CancellationToken cancellationToken = default)
        {
            var validation = validator.Validate(expertSystem);
            if (!validation.Success)
            {
                return new SaveExpertSystemResult
                {
                    Validation = validation
                };
            }

            var name = expertSystem.Name.Trim();
            var duplicate = await db.ExpertSystems
                .AnyAsync(x => x.Id != expertSystem.Id && x.Name == name, cancellationToken);

            if (duplicate)
            {
                validation.Add($"Экспертная система с именем '{name}' уже существует.", nameof(expertSystem.Name));
                return new SaveExpertSystemResult
                {
                    Validation = validation
                };
            }

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (expertSystem.Id > 0)
                {
                    var previous = await db.ExpertSystems
                        .FirstOrDefaultAsync(x => x.Id == expertSystem.Id, cancellationToken);

                    if (previous != null)
                    {
                        previous.Name = await BuildArchivedNameAsync(previous.Name, previous.Id, cancellationToken);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }

                var newExpertSystem = converter.CreateNewDbGraph(expertSystem, name);
                db.ExpertSystems.Add(newExpertSystem);
                await db.SaveChangesAsync(cancellationToken);

                if (expertSystem.Target != null)
                {
                    var target = newExpertSystem.AllVariables
                        .FirstOrDefault(x => x.Name == expertSystem.Target.Name);

                    if (target == null)
                        throw new InvalidOperationException("Не удалось найти целевую переменную в сохранённой ЭС.");

                    newExpertSystem.TargetId = target.Id;
                    await db.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                return new SaveExpertSystemResult
                {
                    Validation = validation,
                    ExpertSystem = await GetAsync(newExpertSystem.Id, cancellationToken)
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new SaveExpertSystemResult
                {
                    Validation = validation,
                    Error = ex.Message
                };
            }
        }

        private async Task<string> BuildArchivedNameAsync(string currentName, int id, CancellationToken cancellationToken)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var candidate = $"{currentName}_auto_{timestamp}";
            var index = 1;

            while (await db.ExpertSystems.AnyAsync(x => x.Id != id && x.Name == candidate, cancellationToken))
                candidate = $"{currentName}_auto_{timestamp}-{index++}";

            return candidate;
        }
    }
}
