using Microsoft.EntityFrameworkCore;

namespace CommonRepositories
{
    /// <summary>
    /// Репозиторий справочника с сортировкой и поиском по имени.
    /// </summary>
    public interface INamedRepository<T> : IRepository<T> where T : BaseEntity
    {
        Task<List<T>> SearchByNameAsync(string query, int limit, int page);

        Task<int> GetCountByNameAsync(string query);
    }


    /// <summary>
    /// Репозиторий справочника с сортировкой и поиском по имени.
    /// Подходит для сущностей, у которых есть строковое свойство Name.
    /// </summary>
    public class NamedRepository<T> : BaseRepository<T>, INamedRepository<T> where T : BaseEntity
    {
        private const string NameProperty = "Name";

        public NamedRepository(DbContext dbContext)
            : base(dbContext)
        {
        }


        protected override IQueryable<T> DefaultOrder(IQueryable<T> set)
        {
            return set.OrderBy(x => EF.Property<string>(x, NameProperty));
        }


        public async Task<List<T>> SearchByNameAsync(string query, int limit, int page)
        {
            query = query ?? string.Empty;

            return await Fetch(
                    DefaultOrder(dbContext.Set<T>())
                    .Where(x => EF.Property<string>(x, NameProperty).Contains(query))
                    .Skip(page * limit)
                    .Take(limit))
                .ToListAsync();
        }


        public async Task<int> GetCountByNameAsync(string query)
        {
            query = query ?? string.Empty;

            return await dbContext.Set<T>()
                .CountAsync(x => EF.Property<string>(x, NameProperty).Contains(query));
        }
    }
}
