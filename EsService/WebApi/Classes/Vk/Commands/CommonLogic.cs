using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using VkBotLib;

namespace WebApi.Classes.Vk.Commands
{
    public class CommonLogic
    {
        public static void SendAboutFilm(string sessionId, FilmDto film, int peerId, VkApiIntegrator vkApi)
        {
            try
            {
                string posterPic = vkApi.PreparePictureAndGetName(film.PosterUrl, peerId);
                vkApi.SendMessage(peerId, "", null, posterPic);
            }
            catch (Exception ex)
            {
                // TODO: постер по-умолчанию, лог
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{WebUtility.HtmlDecode(film.Name)} ({film.Year})");
            sb.AppendLine();
            sb.AppendLine("Осторожно, возможен машинный перевод!"); // а надо сказать, что это машинный перевод?
            sb.AppendLine();
            sb.AppendLine(WebUtility.HtmlDecode($"{film.Description}"));
            sb.AppendLine();
            sb.Append("Основываясь на мнении пользователей, этот фильм: ");
            sb.AppendLine(string.Join(", ", film.CustomProperties.OrderByDescending(x => x.Percent).Select(x => $"на {x.Percent}% {x.Name}")));

            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
                    {
                        new List<Button>() { new Button() { Color = ButtonColor.positive, Label = "Подобрать еще один фильм!", Payload = "Consultation" } },
                        new List<Button>() { new Button() { Color = ButtonColor.secondary, Label = "Другой по тем же параметрам", Payload = $"Another|{sessionId}|{film.Id}" } },
                        //new List<Button>() { new Button() { Color = ButtonColor.secondary, Label = "Оставить отзыв",  Payload = $"Feedback|{sessionId}|{adviceId}|{film.Id}" } }
                        new List<Button>()
                        {
                            new Button() { Color = ButtonColor.secondary, Label = "👎🏻", Payload = $"Review|{sessionId}|{SimpleEstimation.NEGATIVE}" },
                            new Button() { Color = ButtonColor.secondary, Label = "👍🏻", Payload = $"Review|{sessionId}|{SimpleEstimation.POSITIVE}" },
                        }
                    }
            };

            vkApi.SendMessage(peerId, sb.ToString(), keyboard);
        }
    }
}