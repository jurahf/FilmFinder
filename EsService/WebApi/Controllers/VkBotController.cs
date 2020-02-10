using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using WebApi.Classes.Vk;
using WebApi.Classes.Vk.Commands;

namespace WebApi.Controllers
{
    public class VkBotController : ApiController
    {
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
                response.Content = new StringContent("21572c26", Encoding.UTF8, "text/html");
                return response;
            }
            else
            {
                if (message.Type.ToLower() == VkMessageType.Message_new.ToString().ToLower())
                {
                    new CommandContainer().ParseAndDoCommand(message.Object);
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent("ok", Encoding.UTF8, "text/html");
                return response;
            }
        }
    }
}