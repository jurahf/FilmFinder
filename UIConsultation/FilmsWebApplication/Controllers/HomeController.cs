using FilmsWebApplication.Classes;
using FilmsWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FilmsWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private FilmExternalService filmService = new FilmExternalService();    // TODO: DI
        private EsExternalService esService = new EsExternalService();          // TODO: DI


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Questions()
        {
            // отправляем во вью новую сессию, и первый вопрос
            int sessionId = SessionsCache.CreateNewSession();
            VariableDto variable = SessionsCache.NextQuestion(sessionId);
            ViewData["SessionId"] = sessionId;
            ViewData["Variable"] = variable;

            return View();
        }

        public ActionResult IndexContinue(int sessionId, string varName, string varValue)
        {
            // сохраняем результат
            SessionsCache.SetAnswer(sessionId, varName, varValue);

            // если есть еще вопросы - передаем во вью сессию и переменную
            VariableDto variable = SessionsCache.NextQuestion(sessionId);
            if (variable != null)
            {
                ViewData["SessionId"] = sessionId;
                ViewData["Variable"] = variable;
                return View("Questions");
            }
            else
            {
                // если все переменные заполнены - запрашиваем результат в ЭС
                var result = esService.GetConsult(SessionsCache.GetAllAnswers(sessionId));

                // и выводим этот результат
                if (result?.Fact != null && int.TryParse(result.Fact.Value, out int filmId) && filmId > 0)
                {
                    return RedirectToAction(nameof(FilmDetails), new { id = filmId });
                }
                else
                {
                    // (если даже он пустой)
                    return RedirectToAction(nameof(NoFilm));
                }
            }
        }

        // /Home/FilmDetails?id=1
        public ActionResult FilmDetails(int id)
        {
            // запросим данные из БД фильмов
            Film film = filmService.GetFilm(id);
            ViewData["Film"] = film;

            return View();
        }

        public ActionResult NoFilm()
        {
            return View();
        }
    }


    // TODO: ну это вообще, конечно
    public static class SessionsCache
    {
        private static object lockObj = new object();
        private static List<VariableDto> allQuestions = new List<VariableDto>();
        private static Dictionary<int, List<EsVariables>> answers = new Dictionary<int, List<EsVariables>>();

        static SessionsCache()
        {
            EsExternalService esService = new EsExternalService();
            allQuestions = esService.GetQuestions();
        }

        public static int CreateNewSession()
        {
            int sessionId = 0;
            lock (lockObj)
            {
                sessionId = answers.Count + 1;                  // TODO: лучше брать Max
                answers.Add(sessionId, new List<EsVariables>());
            }

            return sessionId;
        }

        public static void SetAnswer(int sessionId, string varName, string varValue)
        {
            if (answers.ContainsKey(sessionId))
            {
                answers[sessionId].Add(new EsVariables() { Variable = varName, Value = varValue });
            }
        }

        public static VariableDto NextQuestion(int sessionId)
        {
            if (answers.ContainsKey(sessionId))
            {
                List<string> usedNames = answers[sessionId].Select(x => x.Variable).ToList();
                foreach (var variable in allQuestions)
                {
                    if (!usedNames.Contains(variable.Name))
                        return variable;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        public static List<EsVariables> GetAllAnswers(int sessionId)
        {
            if (answers.ContainsKey(sessionId))
            {
                return answers[sessionId];
            }
            else
            {
                return new List<EsVariables>();
            }
        }

    }


}