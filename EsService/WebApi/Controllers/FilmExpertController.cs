using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Classes;

namespace WebApi.Controllers
{
    public class FilmExpertController : ApiController
    {
        private DBWork db;
        private EsAlgorithm algorithm;
        private const string esName = "Фильмы";

        public FilmExpertController()
        {
            // TODO: DI
            db = new DBWork();
            algorithm = new EsAlgorithm(db);
        }

        /// <summary>
        /// Создаем сессию и начинаем новую консультацию
        /// </summary>
        /// <returns>Возвращает id сессии и первый вопрос</returns>
        [HttpGet]
        [ActionName("StartConsult")]
        public QuestionOrResultDto StartConsult()
        {
            Session session = algorithm.CreateSessionAndGoConsult(esName);
            var result = algorithm.GetNextQuestionOrResult(session.SessionId);
            return result;
        }


        /// <summary>
        /// Ответ на вопрос и следующий вопрос. Принимает id сессии и ответ
        /// </summary>
        /// <returns>Возвращает следующий вопрос или результат консультации</returns>
        [HttpPost]
        [ActionName("SetAnswerAndNext")]
        public QuestionOrResultDto SetAnswerAndNext([FromBody]AnswerEsArgs answer)
        {
            var result = algorithm.SetAnswerAndGetNextQuestionOrResult(answer.SessionId, answer.VarValue);
            return result;
        }

    }
}
