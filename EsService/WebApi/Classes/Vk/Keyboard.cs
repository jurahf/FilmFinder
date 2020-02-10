using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebApi.Classes.Vk
{
    public class Keyboard
    {
        public bool OneTime { get; set; }
        public bool Inline { get; set; } = true;
        public List<List<Button>> Buttons { get; set; }

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

            return keyboard;
        }

        public string GetJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");

            sb.AppendLine($@"""one_time"": {OneTime.ToString().ToLower()},");

            sb.AppendLine(@"""buttons"": [ ");
            for(int i = 0; i < Buttons.Count; i++)
            {
                List<Button> list = Buttons[i];

                if (i != 0)
                    sb.Append(",");

                sb.AppendLine("[");
                for (int j = 0; j < list.Count; j++)
                {
                    Button btn = list[j];
                    if (j != 0)
                        sb.Append(",");
                    sb.AppendLine("{");
                    sb.AppendLine(@"""action"":");
                    sb.AppendLine("{");
                    sb.AppendLine($@"""type"": ""{btn.Type}"",");
                    sb.AppendLine(@"""payload"": ""{\""command\"":" + $@"\""{btn.Payload}\""" + @"}"",");
                    sb.AppendLine($@"""label"": ""{btn.Label}""");
                    sb.AppendLine("}");
                    sb.Append(",");
                    sb.AppendLine($@"""color"": ""{btn.Color}""");
                    sb.AppendLine("}");
                }
                sb.AppendLine("]");
            }
            sb.AppendLine("],");

            sb.AppendLine($@"""inline"": {Inline.ToString().ToLower()}");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }


    public class Button // по сути, это только Text-action, есть и другие типы кнопок, у которых другие поля
    {
        public string Type => "text";
        public string Payload { get; set; }
        public string Label { get; set; }
        public ButtonColor Color { get; set; }
    }

    public enum ButtonColor
    {
        primary,
        secondary,
        positive,
        negative
    }

}