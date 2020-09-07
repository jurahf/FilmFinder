using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VkBotLib;

namespace WebApi.Classes.Vk.Commands
{
    public class ReviewCommand : Command
    {
        private DBWork db;

        public ReviewCommand(VkApiIntegrator vkApi)
            : base(vkApi)
        {
            db = new DBWork();
        }

        public override string Description => "Простая оценка: положительно или отрицательно";

        public override List<string> Names => new List<string>()
        {
            "Review"
        };

        public override void Do(VkPersonMessage message)
        {
            string[] arr = message.TextOrPayload.Split('|');
            string sessionId = ClearMessage(arr[1]);
            string estimation = ClearMessage(arr[2]);

            Session session = db.GetFromDatabase<Session>(x => x.SessionId == sessionId).FirstOrDefault();

            if (session != null)
            {
                Review review = new Review()
                {
                    Session = session,
                    Comment = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} - {estimation}",
                    Estimate = SimpleEstimation.ToInt(estimation)
                };

                db.Insert(review);
            }

            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
                {
                    new List<Button>() { new Button() { Color = ButtonColor.positive, Label = "Подобрать еще один фильм!", Payload = "Consultation" } },
                }
            };

            vkApi.SendMessage(message.Peer_Id, "Спасибо за оценку!", keyboard);
        }
    }

    public static class SimpleEstimation
    {
        public const string POSITIVE = "Positive";
        public const string NEGATIVE = "Negative";

        public static int ToInt(string value)
        {
            if (value.ToLower() == POSITIVE.ToLower())
                return 10;
            else if (value.ToLower() == NEGATIVE.ToLower())
                return 0;
            else
                throw new ArgumentOutOfRangeException("Нераспознанная оценка");
        }
    }


}