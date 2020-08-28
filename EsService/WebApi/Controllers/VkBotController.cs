using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using VkBotLib;
using WebApi.Classes.Vk;
using WebApi.Classes.Vk.Commands;

namespace WebApi.Controllers
{
    public class VkBotController : ApiController
    {
        private const string vkToken = "89dbe5916cca655ab346340a0c1df410568552a6b3d248155ae6ba64a1d1d5b8a2fb662a98817937070c3";
        private const string vkConfirmKey = "b2ee4209";

        public VkBotController()
        {
            // для https
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        [HttpPost]
        [ActionName("VkMessageAccept")]
        public HttpResponseMessage VkMessageAccept([FromBody]VkMessage message)
        {
            //Logger.Log("Message", message.Type);

            if (message.Type.ToLower() == VkMessageType.Confirmation.ToString().ToLower())
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(vkConfirmKey, Encoding.UTF8, "text/html");
                return response;
            }
            else
            {
                Task.Factory.StartNew(() => DoWork(message));

                return SendOk();
            }
        }

        private void DoWork(VkMessage message)
        {
            if (message.Type.ToLower() == VkMessageType.Message_new.ToString().ToLower())
            {
                new CommandContainer(RegisterCommands()).ParseAndDoCommand(message.Object);
            }
        }

        private HttpResponseMessage SendOk()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("ok", Encoding.UTF8, "text/html");
            return response;
        }


        private List<Command> RegisterCommands()
        {
            var vkApi = new VkApiIntegrator(vkToken);

            var commandsList = new List<Command>()
            {
                new StartCommand(vkApi),
                new StartConsultationCommand(vkApi),
                new SetAnswerAndNextCommand(vkApi),
                new AnotherCommand(vkApi)
            };

            return commandsList;
        }

        //[HttpGet]
        //[ActionName("TestPicture")]
        //public string TestPicture(string url, int peerId)
        //{
        //    VkApiIntegrator vkApi = new VkApiIntegrator();
        //    string picName = vkApi.PreparePictureAndGetName(url, peerId);
        //    vkApi.SendMessage(peerId, "Test", null, picName);
        //    return picName;
        //}

    }
}