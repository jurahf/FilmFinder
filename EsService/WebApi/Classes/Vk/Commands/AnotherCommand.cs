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
            int adviceId = int.Parse(ClearMessage(arr[2]));
            int filmId = int.Parse(ClearMessage(arr[3]));

            // надо найти следующий в том же совете
            var advice = db.GetFromDatabase<Advice>(x => x.Id == adviceId).FirstOrDefault();
            if (advice == null)
                return;

            vkApi.SendMessage(message.Peer_Id, "Сейчас поищу...", keyboard: null);

            List<Film> filmList = new FilmAndAdviceLogic().FindFilmsByAdvice(advice);

            CommonLogic.SendAboutFilm(sessionId, new FilmDto(filmList.First()), adviceId, message.Peer_Id, vkApi);
        }
    }
}