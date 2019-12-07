using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class EsAlgorithm
    {
        private DBWork db;
        private SessionLogic sessionLogic;
        private EsLogic esLogic;

        public EsAlgorithm(DBWork db)
        {
            this.db = db;
            sessionLogic = new SessionLogic(db);
            esLogic = new EsLogic(db);
        }


        public Session CreateSessionAndGoConsult(string esName)
        {
            Session session = sessionLogic.CreateNewSession();
            ExpertSystem es = db.GetFromDatabase<ExpertSystem>(x => x.Name == esName).FirstOrDefault();
            if (es == null)
                throw new ArgumentOutOfRangeException($"Не найдено экспертной системы с именем {esName}");

            esLogic.CreateConsult(session, es);

            return session;
        }



        public QuestionOrResultDto GetNextQuestionOrResult(string sessionId)
        {
            Consultation consult = FindConsultation(sessionId);
            Session session = consult.Session;
            sessionLogic.UpdateActivityDate(session);

            FactQuestionOrResult qor = esLogic.DoConsult(consult);

            return CreateQuestionOrResultDto(sessionId, consult, qor);
        }

        public QuestionOrResultDto SetAnswerAndGetNextQuestionOrResult(string sessionId, VariableValue answer)
        {
            Consultation consult = FindConsultation(sessionId);
            Session session = consult.Session;
            sessionLogic.UpdateActivityDate(session);

            FactQuestionOrResult qor = esLogic.ContinueConsult(consult, answer);

            return CreateQuestionOrResultDto(sessionId, consult, qor);
        }
        
        private Consultation FindConsultation(string sessionId)
        {
            return db.GetFromDatabase<Consultation>(x => x.Session.SessionId == sessionId).First();
        }


        private QuestionOrResultDto CreateQuestionOrResultDto(string sessionId, Consultation consult, FactQuestionOrResult qor)
        {
            if (qor.Result != null)
            {
                return new QuestionOrResultDto(sessionId)
                {
                    Result = new ConsultResultDto(qor.Result.Fact, consult.WorkedRules.ToList())
                };
            }
            else
            {
                return new QuestionOrResultDto(sessionId)
                {
                    Question = new VariableDto(qor.Question)
                };
            }
        }
    }
}
