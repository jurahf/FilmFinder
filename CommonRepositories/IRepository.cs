using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRepositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        List<T> GetAll(int limit, int page);

        List<T> GetAll();

        T GetById(long id);

        void Update(T entity);

        void UpdateAll(List<T> entities);

        void Add(T entity);

        void Delete(long id);

        int GetCount();
    }
}
