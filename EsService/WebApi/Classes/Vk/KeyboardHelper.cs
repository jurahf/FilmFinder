using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VkBotLib;

namespace WebApi.Classes.Vk
{
    public static class KeyboardHelper
    {
        public static Keyboard CreateFromVariable(string sessionId, VariableDto variable)
        {
            Keyboard keyboard = new Keyboard()
            {
                OneTime = false,
                Buttons = new List<List<Button>>()
            };

            // TODO: здесь формат чисто строковый, а потом он парсится в SetAnswerAndNext
            foreach (var variant in variable.Domain)
            {
                keyboard.Buttons.Add(new List<Button>()
                {
                    new Button() { Payload = $"SetAnswerAndNext|{sessionId}|{variable.Name}|{variant}", Label = variant, Color = ButtonColor.primary }
                });
            }

            keyboard.Buttons.Add(new List<Button>() {
                new Button() { Color = ButtonColor.negative, Label = "Начать подбор заново", Payload = "Consultation" }
            });

            return keyboard;
        }
    }
}