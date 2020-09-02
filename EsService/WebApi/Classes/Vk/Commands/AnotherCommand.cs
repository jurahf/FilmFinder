using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VkBotLib;

namespace WebApi.Classes.Vk.Commands
{
    public class AnotherCommand : Command
    {
        private DBWork db;

        public AnotherCommand(VkApiIntegrator vkApi)
            : base(vkApi)
        {
            db = new DBWork();
        }

        public override string Description => "Показать другой подходящий фильм по этой же консультации";

        public override List<string> Names => new List<string>
        {
            "Another"
        };

        public override void Do(VkPersonMessage message)
        {
            // TODO: обработка ошибок
            string[] arr = message.TextOrPayload.Split('|');
            string sessionId = ClearMessage(arr[1]);
            int filmId = int.Parse(ClearMessage(arr[2]));

            Consultation consult = db.GetFromDatabase<Consultation>(x => x.Session.SessionId == sessionId).FirstOrDefault();
            if (consult?.FinalSolution == null)
                throw new ArgumentException("Не найдены результаты консультации");

            vkApi.SendMessage(message.Peer_Id, "Сейчас поищу...", keyboard: null);


            var film = new FilmAndAdviceLogic().FindFilmByConsultResult(new ConsultResultDto()
            {
                Fact = new FactDto()
                {
                    VarName = consult.FinalSolution.VariableName,
                    Value = consult.FinalSolution.Value
                }
            });

            CommonLogic.SendAboutFilm(sessionId, film, message.Peer_Id, vkApi);
        }
    }
}