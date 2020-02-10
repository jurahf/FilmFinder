using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Classes.Vk.Commands
{
    public class StartCommand : Command
    {
        public StartCommand(VkApiIntegrator vkApi)
            : base(vkApi)
        {
        }

        public override string Description => "Приветствие и начало общения с ботом";

        public override List<string> Names => new List<string>()
        {
            "start",
        };

        public override void Do(VkPersonMessage message)
        {
            string hello = "Привет! Я помогу подобрать фильм под ситуацию и настроение.";
            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
                {
                    new List<Button>() { new Button() { Color = ButtonColor.positive, Label = "Подобрать!", Payload = "Consultation" } }
                }
            };

            vkApi.SendMessage(message.Peer_Id, hello, keyboard);
        }
    }
}