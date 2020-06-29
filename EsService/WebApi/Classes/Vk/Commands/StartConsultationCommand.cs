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
    public class StartConsultationCommand : Command
    {
        // !!! Копирует FilmExpertController
        private DBWork db;
        private EsAlgorithm algorithm;
        private const string esName = "Фильмы";

        public StartConsultationCommand(VkApiIntegrator vkApi)
            : base(vkApi)
        {
            db = new DBWork();
            algorithm = new EsAlgorithm(db);
        }

        public override string Description => "Начинает новую сессию консультации с ботом";

        public override List<string> Names => new List<string>()
        {
            "Consultation"
        };



        public override void Do(VkPersonMessage message)
        {
            Session session = algorithm.CreateSessionAndGoConsult(esName);
            QuestionOrResultDto result = algorithm.GetNextQuestionOrResult(session.SessionId);

            Keyboard keyboard = KeyboardHelper.CreateFromVariable(result.SessionId, result.Question);

            vkApi.SendMessage(message.Peer_Id, result.Question.Question, keyboard);
        }



    }
}