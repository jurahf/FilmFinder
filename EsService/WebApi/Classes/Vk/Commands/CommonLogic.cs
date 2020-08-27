﻿using ExpertSystemDb.DTOs;
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
        public static void SendAboutFilm(string sessionId, FilmDto film, int adviceId, int peerId, VkApiIntegrator vkApi)
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
            sb.AppendLine(WebUtility.HtmlDecode(film.Description));

            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
                    {
                        new List<Button>() { new Button() { Color = ButtonColor.positive, Label = "Подобрать еще один фильм!", Payload = "Consultation" } },
                        new List<Button>() { new Button() { Color = ButtonColor.secondary, Label = "Другой по тем же параметрам", Payload = $"Another|{sessionId}|{adviceId}|{film.Id}" } },
                        //new List<Button>() { new Button() { Color = ButtonColor.secondary, Label = "Оставить отзыв",  Payload = $"Feedback|{sessionId}|{adviceId}|{film.Id}" } }
                    }
            };

            vkApi.SendMessage(peerId, sb.ToString(), keyboard);
        }
    }
}