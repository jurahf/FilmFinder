using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonRepositories
{
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly DbContext dbContext;

        public BaseRepository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected virtual IQueryable<T> Fetch(IQueryable<T> set)
        {
            return set;
        }

        protected virtual IQueryable<T> DefaultOrder(IQueryable<T> set)
        {
            return set.OrderBy(x => x.Id);
        }

        public async virtual Task<int> GetCountAsync()
        {
            try
            {
                return await dbContext.Set<T>().CountAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetCountAsync)}");
                throw;
            }
        }

        public async virtual Task<List<T>> GetAllAsync(int limit = 100, int page = 0)
        {
            try
            {
                return await Fetch(
                        DefaultOrder(dbContext.Set<T>())
                        .Skip(page * limit)
                        .Take(limit))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetAllAsync)}");
                throw;
            }
        }

        public async virtual Task<List<T>> GetAllAsync()
        {
            try
            {
                return await Fetch(
                        DefaultOrder(dbContext.Set<T>()))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetAllAsync)}");
                throw;
            }
        }

        public async virtual Task<T> GetByIdAsync(long id)
        {
            try
            {
                return await Fetch(dbContext.Set<T>())
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetByIdAsync)}");
                throw;
            }
        }

        public async virtual Task AddAsync(T entity)
        {
            try
            {
                await dbContext.Set<T>().AddAsync(entity);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(AddAsync)}");
                throw;
            }
        }

        public async virtual Task DeleteAsync(long id)
        {
            try
            {
                T entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    dbContext.Set<T>().Remove(entity);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(DeleteAsync)}");
                throw;
            }
        }

        public async virtual Task UpdateAsync(T entity)
        {
            try
            {
                if (entity.Id > 0 && dbContext.Set<T>().Any(x => x.Id == entity.Id))
                {
                    if (dbContext.Entry(entity).State == EntityState.Detached)
                        dbContext.Entry(entity).State = EntityState.Modified;
                    dbContext.Update(entity);
                }
                else
                {
                    await dbContext.AddAsync(entity);
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(UpdateAsync)}");
                throw;
            }
        }

        public async virtual Task UpdateAllAsync(List<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    if (entity.Id > 0 && dbContext.Set<T>().Any(x => x.Id == entity.Id))
                    {
                        if (dbContext.Entry(entity).State == EntityState.Detached)
                            dbContext.Entry(entity).State = EntityState.Modified;
                        dbContext.Update(entity);
                    }
                    else
                    {
                        await dbContext.AddAsync(entity);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(UpdateAllAsync)}");
                throw;
            }
        }
    }
}
