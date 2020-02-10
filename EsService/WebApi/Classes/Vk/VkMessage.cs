using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Classes.Vk
{
    public enum VkMessageType
    {
        Confirmation,
        Message_new,
    }

    public class VkMessage
    {
        public string Type { get; set; }
        public string Group_id { get; set; }
        public string Secret { get; set; }
        public VkPersonMessage Object { get; set; }
    }

    public class VkPersonMessage
    {
        public int Id { get; set; }
        public int Date { get; set; }
        public int Peer_Id { get; set; }
        public int From_Id { get; set; }
        public string Text { get; set; }
        public string Payload { get; set; }

        public string TextOrPayload
        {
            get
            {
                if (!string.IsNullOrEmpty(Payload))
                    return ParsePayload(Payload);   // TODO: сделать не так костыльно
                else
                    return Text;
            }
        }

        private string ParsePayload(string payload)
        {
            return JsonConvert.DeserializeObject<Payload>(payload)?.Command;
        }
    }

    public class Payload
    {
        public string Command { get; set; }
    }
}