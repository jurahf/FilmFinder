using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using VkBotLib;

namespace WebApi.Classes.Vk.Commands
{
    public class SetAnswerAndNextCommand : Command
    {
        // !!! Копирует FilmExpertController и частично FilmAdviceController
        private DBWork db;
        private EsAlgorithm algorithm;

        public SetAnswerAndNextCommand(VkApiIntegrator vkApi)
            : base(vkApi)
        {
            db = new DBWork();
            algorithm = new EsAlgorithm(db);
        }

        public override string Description => "Ответ на вопрос и переход к следующему вопросу, либо концу консультации";

        public override List<string> Names => new List<string>()
        {
            "SetAnswerAndNext"
        };

        public override void Do(VkPersonMessage message)
        {
            // TODO: неплохо бы от ошибок защититься
            string[] arr = message.TextOrPayload.Split('|');
            string sessionId = ClearMessage(arr[1]);
            string variable = ClearMessage(arr[2]);
            string value = ClearMessage(arr[3]);
            var answer = new VariableValue()
            {
                Variable = variable,
                Value = value,
            };

            QuestionOrResultDto result = algorithm.SetAnswerAndGetNextQuestionOrResult(sessionId, answer);

            if (result.Question != null)
            {
                // следующий вопрос
                Keyboard keyboard = KeyboardHelper.CreateFromVariable(result.SessionId, result.Question);

                vkApi.SendMessage(message.Peer_Id, result.Question.Question, keyboard);
            }
            else
            {
                // готов ответ, получим подробности
                string key = result.Result.Fact.Value;
                Guid guid;
                if (!Guid.TryParse(key, out guid))
                {
                    SendError(message.Peer_Id);
                    return;
                }

                Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

                if (advice == null)
                {
                    SendError(message.Peer_Id);
                    return;
                }

                vkApi.SendMessage(message.Peer_Id, "Сейчас что-нибудь подберу...", keyboard: null);

                var filmList = new FilmAndAdviceLogic().FindFilmsByAdvice(advice);
                var filmDtos = filmList.Select(x => new FilmDto(x)).ToList();

                int index = new Random().Next(0, filmDtos.Count - 1);
                var film = filmDtos[index];

                CommonLogic.SendAboutFilm(sessionId, film, advice.Id, message.Peer_Id, vkApi);
            }
        }


        private void SendError(int peerId)
        {
            string message = "К сожалению, в работе сервиса произошла ошибка. Вы можете повторить попытку позже.";
            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
                {
                    new List<Button>() { new Button() { Color = ButtonColor.positive, Label = "Попробовать еще раз", Payload = "Consultation" } }
                }
            };

            vkApi.SendMessage(peerId, message, keyboard);
        }


    }
}