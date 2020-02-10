using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebApi.Classes.Vk.Commands
{
    public abstract class Command
    {
        protected VkApiIntegrator vkApi;

        public Command(VkApiIntegrator vkApi)
        {
            this.vkApi = vkApi;
        }

        public virtual bool IsCalled(string text)
        {
            if (text == null)
                return false;

            string msg = ClearMessage(text);

            return Names.Select(n => n.ToLower()).Any(n => msg.StartsWith(n));
        }

        protected string ClearMessage(string text)
        {
            string msg = text.Trim().ToLower();
            msg = Regex.Replace(msg, @"^\[.+?\]", "");
            msg = msg.Trim(new char[] { ' ', ',', '\\', '}', '{', '"', '\'' });

            return msg;
        }


        public abstract string Description { get; }
        public abstract List<string> Names { get; }
        public abstract void Do(VkPersonMessage message);
    }
}