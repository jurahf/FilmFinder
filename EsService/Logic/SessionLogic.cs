using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class SessionLogic
    {
        private DBWork db;

        public SessionLogic(DBWork db)
        {
            this.db = db;
        }

        /// <summary>
        /// Создает но не сохраняет новую сессию
        /// </summary>
        /// <returns></returns>
        public Session CreateNewSession()
        {
            Session session = new Session()
            {
                SessionId = Guid.NewGuid().ToString(),
                CreateDate = DateTime.Now
            };

            //db.Insert(session);

            return session;
        }

        /// <summary>
        /// Обновляет дату активности у сессии
        /// </summary>
        /// <param name="session"></param>
        public void UpdateActivityDate(Session session)
        {
            session.LastActivityDate = DateTime.Now;
            db.Update(session);
        }


    }
}
