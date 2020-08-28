using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertSystemDb
{
    public class DBWork : IDataWork
    {
        private const string idFieldName = "Id";
        private ExpertSystemModelContainer database = null;

        private ExpertSystemModelContainer Database
        {
            get
            {
                return database;
            }
        }


        public DBWork()
        {
            database = new ExpertSystemModelContainer();
        }
             


        private int GetId<T>(T obj) where T : class
        {
            return (int)typeof(T).GetProperty(idFieldName).GetValue(obj, null);
        }

        public int GetCount<T>() where T : class
        {
            try
            {
                return Database.Set<T>().Count();
            }
            catch
            {
                throw;
            }
        }

        public int GetCount<T>(Func<T, bool> whereClause) where T : class
        {
            try
            {
                return Database.Set<T>().Where(whereClause).Count();
            }
            catch
            {
                throw;
            }
        }

        public List<T> GetFromDatabase<T>() where T : class
        {
            try
            {
                return Database.Set<T>().ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<T> GetFromDatabase<T>(Func<T, bool> filter) where T : class
        {
            try
            {
                return Database.Set<T>().Where(filter).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<object> GetFromDatabase(Type type)
        {
            try
            {
                var set = Database.Set(type);
                List<object> res = new List<object>();

                foreach (var e in set)
                {
                    res.Add(e);
                }

                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void DeleteObject<T>(T obj) where T : class
        {
            try
            {
                Database.Set<T>().Remove(obj);

                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void DeleteObject<T>(List<T> objList) where T : class
        {
            try
            {
                foreach (var obj in objList)
                {
                    Database.Set<T>().Remove(obj);
                }

                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Update<T>(T obj) where T : class
        {
            try
            {
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Update<T>(List<T> objList) where T : class
        {
            Update(objList.FirstOrDefault());
        }

        public void Insert<T>(T obj) where T : class
        {
            try
            {
                // объект мог сохраниться с другими объектами, поэтому добавлять его не всегда надо
                if (GetFromDatabase<T>().FirstOrDefault(x => GetId(x) == GetId(obj)) == null) // костыль и долго, но зато ошибок с двойным сохранением точно не будет
                {
                    Database.Set<T>().Add(obj);
                }

                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public void Insert<T>(List<T> objList) where T : class
        {
            try
            {
                foreach (var obj in objList)
                {
                    // объект мог сохраниться с другими объектами, поэтому добавлять его не всегда надо
                    if (GetFromDatabase<T>().FirstOrDefault(x => GetId(x) == GetId(obj)) == null) // костыль и долго, но зато ошибок с двойным сохранением точно не будет
                    {
                        Database.Set<T>().Add(obj);
                    }
                }

                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public void AddWithoutSave<T>(T obj) where T : class
        {
            try
            {
                Database.Set<T>().Add(obj);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void AddWithoutSave<T>(List<T> objList) where T : class
        {
            try
            {
                Database.Set<T>().AddRange(objList); // рискуем
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public void Save()
        {
            Database.SaveChanges();
        }



    }
}
