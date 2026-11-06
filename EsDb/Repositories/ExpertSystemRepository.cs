using CommonRepositories;
using EsDb.Model;
using Microsoft.EntityFrameworkCore;

namespace EsDb.Repositories
{
    public class ExpertSystemRepository : BaseRepository<ExpertSystem>, IExpertSystemRepository
    {
        private readonly EsDbContext db;

        public ExpertSystemRepository(EsDbContext db)
            : base(db)
        {
            this.db = db;
        }

        protected override IQueryable<ExpertSystem> Fetch(IQueryable<ExpertSystem> set)
        {
            return set
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.AllDomains)
                    .ThenInclude(x => x.DomainValue)
                .Include(x => x.AllVariables)
                    .ThenInclude(x => x.Domain)
                .Include(x => x.AllRules)
                    .ThenInclude(x => x.Conditions)
                        .ThenInclude(x => x.Fact)
                            .ThenInclude(x => x.Variable)
                .Include(x => x.AllRules)
                    .ThenInclude(x => x.Conditions)
                        .ThenInclude(x => x.Fact)
                            .ThenInclude(x => x.DomainValue)
                .Include(x => x.AllRules)
                    .ThenInclude(x => x.Result)
                        .ThenInclude(x => x!.Variable)
                .Include(x => x.AllRules)
                    .ThenInclude(x => x.Result)
                        .ThenInclude(x => x!.DomainValue)
                .Include(x => x.Target);
        }

        public Task<ExpertSystem?> GetGraphAsync(int id)
        {
            return Fetch(db.ExpertSystems).FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<ExpertSystem?> GetGraphByNameAsync(string name)
        {
            return Fetch(db.ExpertSystems).FirstOrDefaultAsync(x => x.Name == name);
        }

        public Task<List<ExpertSystemListRow>> GetListAsync()
        {
            return db.ExpertSystems
                .AsNoTracking()
                .Select(x => new ExpertSystemListRow
                {
                    Id = x.Id,
                    Name = x.Name,
                    DomainCount = x.AllDomains.Count,
                    VariableCount = x.AllVariables.Count,
                    RuleCount = x.AllRules.Count
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        public Task<List<Fact>> GetFactsAsync(int expertSystemId)
        {
            return db.Facts
                .AsNoTracking()
                .Include(x => x.Variable)
                .Include(x => x.DomainValue)
                .Where(x => x.Variable.ExpertSystemId == expertSystemId)
                .ToListAsync();
        }
    }
}
