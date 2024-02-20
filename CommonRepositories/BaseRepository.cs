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

        public virtual int GetCount()
        {
            try
            {
                return dbContext.Set<T>().Count();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetCount)}");
                throw;
            }
        }

        public virtual List<T> GetAll(int limit = 100, int page = 0)
        {
            try
            {
                return Fetch(
                        DefaultOrder(dbContext.Set<T>())
                        .Skip(page * limit)
                        .Take(limit))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetAll)}");
                throw;
            }
        }

        public virtual List<T> GetAll()
        {
            try
            {
                return Fetch(
                        DefaultOrder(dbContext.Set<T>()))
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetAll)}");
                throw;
            }
        }

        public virtual T GetById(long id)
        {
            try
            {
                return Fetch(dbContext.Set<T>())
                    .FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(GetById)}");
                throw;
            }
        }

        public virtual void Add(T entity)
        {
            try
            {
                dbContext.Set<T>().Add(entity);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(Add)}");
                throw;
            }
        }

        public virtual void Delete(long id)
        {
            try
            {
                T entity = GetById(id);
                if (entity != null)
                {
                    dbContext.Set<T>().Remove(entity);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(Delete)}");
                throw;
            }
        }

        public virtual void Update(T entity)
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
                    dbContext.Add(entity);
                }

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(Update)}");
                throw;
            }
        }

        public virtual void UpdateAll(List<T> entities)
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
                        dbContext.Add(entity);
                    }
                }

                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Ошибка в {nameof(UpdateAll)}");
                throw;
            }
        }
    }
}
