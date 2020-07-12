using ExpertSystemDb;
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
            int adviceId = int.Parse(ClearMessage(arr[1]));
            int filmId = int.Parse(ClearMessage(arr[2]));

            // надо найти следующий в том же совете
            var advice = db.GetFromDatabase<Advice>(x => x.Id == adviceId).FirstOrDefault();
            if (advice == null)
                return;

            List<Film> filmList = new FilmAndAdviceLogic().FindFilmsByAdvice(advice);
            int index = filmList.FindIndex(x => x.Id == filmId);
            index++;
            if (index >= filmList.Count)
                index = 0;

            CommonLogic.SendAboutFilm(new FilmDto(filmList[index]), adviceId, message.Peer_Id, vkApi);
        }
    }
}