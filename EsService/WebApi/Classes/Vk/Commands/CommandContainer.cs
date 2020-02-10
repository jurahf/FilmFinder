using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Classes.Vk.Commands
{
    public class CommandContainer
    {
        private List<Command> commandsList;

        public CommandContainer()
        {
            RegisterCommands();
        }

        public void ParseAndDoCommand(VkPersonMessage message)
        {
            foreach (var command in commandsList) // это не слишком долго?
            {
                if (CheckAndDoCommand(message.TextOrPayload, command, message))
                    break;
            }
        }


        private bool CheckAndDoCommand(string commandName, Command command, VkPersonMessage message)
        {
            if (command.IsCalled(commandName))    // просто текстом
            {
                //Logger.Log(command.GetType().Name, $"Peer_Id: {message.Peer_Id}, FromId: {message.From_Id}, message: {message.Text}");
                command.Do(message);
                return true;
            }

            return false;
        }

        private void RegisterCommands()
        {
            var vkApi = new VkApiIntegrator();

            commandsList = new List<Command>()
            {
                new StartCommand(vkApi),
                new StartConsultationCommand(vkApi),
                new SetAnswerAndNextCommand(vkApi),
                new AnotherCommand(vkApi)
            };
        }



    }
}