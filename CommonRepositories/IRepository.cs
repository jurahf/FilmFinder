using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRepositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<List<T>> GetAllAsync(int limit, int page);

        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(long id);

        Task UpdateAsync(T entity);

        Task UpdateAllAsync(List<T> entities);

        Task AddAsync(T entity);

        Task DeleteAsync(long id);

        Task<int> GetCountAsync();
    }
}
