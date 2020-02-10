using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebApi.Classes.Vk.Commands
{
    public class CommonLogic
    {
        public static void SendAboutFilm(FilmDto film, int adviceId, int peerId, VkApiIntegrator vkApi)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{film.Name} ({film.Year})");
            sb.AppendLine(film.Description);

            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
                    {
                        new List<Button>() { new Button() { Color = ButtonColor.positive, Label = "Подобрать еще один фильм!", Payload = "Consultation" } },
                        new List<Button>() { new Button() { Color = ButtonColor.secondary, Label = "Другой такой же", Payload = $"Another|{adviceId}|{film.Id}" } }
                    }
            };

            vkApi.SendMessage(peerId, sb.ToString(), keyboard);
        }
    }
}