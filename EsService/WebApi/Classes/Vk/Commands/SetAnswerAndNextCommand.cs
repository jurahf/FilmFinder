using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

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
            string[] arr = message.Payload.Split('|');
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
                Keyboard keyboard = Keyboard.CreateFromVariable(result.SessionId, result.Question);

                vkApi.SendMessage(message.Peer_Id, result.Question.Question, keyboard);
            }
            else
            {
                // готов ответ, получим подробности
                string key = result.Result.Fact.Value;
                Guid guid;
                if (!Guid.TryParse(key, out guid))
                    SendError(message.Peer_Id);

                Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

                if (advice == null)
                    SendError(message.Peer_Id);

                var dto = new AdviceDto(advice);
                var film = dto.Films[0];

                CommonLogic.SendAboutFilm(film, advice.Id, message.Peer_Id, vkApi);
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