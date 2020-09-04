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
            List<Button> currentRow = new List<Button>();
            foreach (var variant in variable.Domain)
            {
                if (variant.StartNewLint)
                {
                    if (currentRow.Any())
                        keyboard.Buttons.Add(currentRow);

                    currentRow = new List<Button>();
                }

                currentRow.Add(
                    new Button() { Payload = $"SetAnswerAndNext|{sessionId}|{variable.Name}|{variant.Value}", Label = variant.Value, Color = MapButtonColor(variant.Color) }
                );
            }

            if (currentRow.Any())
            {
                keyboard.Buttons.Add(currentRow);
            }

            keyboard.Buttons.Add(new List<Button>() {
                new Button() { Color = ButtonColor.negative, Label = "Начать подбор заново", Payload = "Consultation" }
            });

            return keyboard;
        }




        private static ButtonColor MapButtonColor(DomainValueColor mood)
        {
            switch (mood)
            {
                case DomainValueColor.Blue:
                    return ButtonColor.primary;
                case DomainValueColor.White:
                    return ButtonColor.secondary;
                case DomainValueColor.Green:
                    return ButtonColor.positive;
                case DomainValueColor.Red:
                    return ButtonColor.negative;
                default:
                    return ButtonColor.primary;
            }
        }
    }
}