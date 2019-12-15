using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpertSystemDb
{
    public interface IDataWork
    {
        int GetCount<T>() where T : class;
        int GetCount<T>(Func<T, bool> whereClause) where T : class;

        List<T> GetFromDatabase<T>() where T : class;
        List<object> GetFromDatabase(Type type);
        List<T> GetFromDatabase<T>(Func<T, bool> whereClause) where T : class;

        void Insert<T>(T obj) where T : class;
        void Insert<T>(List<T> objList) where T : class;

        void Update<T>(T obj) where T : class;
        void Update<T>(List<T> objList) where T : class;

        void DeleteObject<T>(T obj) where T : class;
        void DeleteObject<T>(List<T> objList) where T : class;
    }
}
