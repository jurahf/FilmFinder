using ConsultationWeb.Classes;
using ConsultationWeb.Classes.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ConsultationWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StartConsultation()
        {
            ExpertSystemExternalService service = new ExpertSystemExternalService(); // TODO: DI
            QuestionOrResultDto start = service.StartConsult();

            if (start != null)
            {
                ViewData["SessionId"] = start.SessionId;
                ViewData["Quest"] = start.Question;

                return View("Question");
            }
            else
            {
                return View("Error");
            }
        }


        public ActionResult Answer(string sessionId, string varName, string varValue)
        {
            FilmExternalService serviceFilm = new FilmExternalService();        // TODO: DI
            ExpertSystemExternalService serviceEs = new ExpertSystemExternalService(); // TODO: DI
            QuestionOrResultDto dto = serviceEs.SetAnswerAndNext(sessionId, varName, varValue);

            if (dto != null)
            {
                if (dto.Question != null)
                {
                    ViewData["SessionId"] = dto.SessionId;
                    ViewData["Quest"] = dto.Question;

                    return View("Question");
                }
                else //  result.Result != null
                {
                    ViewData["SessionId"] = dto.SessionId;

                    // пришел совет, а по нему надо получить фильмы
                    var advice = serviceFilm.GetAdviceDetails(dto.Result.Fact.Value); // это гуид совета
                    ViewData["Advice"] = advice;

                    return View("ResultFilms");
                }
            }
            else
            {
                return View("Error");
            }
        }



    }
}