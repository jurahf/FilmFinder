using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace WebApi.Classes.Vk
{
    public class VkApiIntegrator
    {
        private const string token = "89dbe5916cca655ab346340a0c1df410568552a6b3d248155ae6ba64a1d1d5b8a2fb662a98817937070c3";


        public void SendMessage(int peer_id, string msg, Keyboard keyboard)
        {
            // https://api.vk.com/method/METHOD_NAME?PARAMETERS&access_token=ACCESS_TOKEN&v=V
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.vk.com/");

                int rand_id = GenerateRandId();

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("access_token", token));
                keyValues.Add(new KeyValuePair<string, string>("random_id", rand_id.ToString()));
                keyValues.Add(new KeyValuePair<string, string>("message", msg));
                keyValues.Add(new KeyValuePair<string, string>("peer_id", peer_id.ToString()));
                keyValues.Add(new KeyValuePair<string, string>("v", "5.90"));
                keyValues.Add(new KeyValuePair<string, string>("keyboard", keyboard.GetJson()));
                var content = new FormUrlEncodedContent(keyValues);

                HttpResponseMessage response = client.PostAsync("method/messages.send", content).Result;
            }
        }

        public string GetMessageUploadUrl(int peer_id)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.vk.com/");

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("access_token", token));
                keyValues.Add(new KeyValuePair<string, string>("type", "doc"));
                keyValues.Add(new KeyValuePair<string, string>("peer_id", peer_id.ToString()));
                keyValues.Add(new KeyValuePair<string, string>("v", "5.90"));
                var content = new FormUrlEncodedContent(keyValues);

                HttpResponseMessage response = client.PostAsync("method/docs.getMessagesUploadServer", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = response.Content.ReadAsStringAsync().Result;
                    UploadServer server = JsonConvert.DeserializeObject<UploadServer>(jsonContent);
                    if (string.IsNullOrWhiteSpace(server?.Response?.Upload_Url))
                        throw new Exception(server.Error);
                    else
                        return server.Response.Upload_Url;
                }
                else
                    throw new Exception();
            }
        }

        public PhotoInfo UploadPhoto(string url, byte[] fileBytes)
        {
            using (HttpClient client = new HttpClient())
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                //form.Add(new StringContent(token), "access_token");
                form.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), "photo");

                HttpResponseMessage response = client.PostAsync(url, form).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = response.Content.ReadAsStringAsync().Result;
                    UploadPhotoInfo uploadInfo = JsonConvert.DeserializeObject<UploadPhotoInfo>(jsonContent);
                    if (uploadInfo?.Photo == null)
                        throw new Exception(uploadInfo.Error);
                    else
                        return uploadInfo.Photo;
                }
                else
                    throw new Exception();
            }
        }


        private int GenerateRandId()
        {
            return Guid.NewGuid().GetHashCode();
        }

    }

    public class UploadServer
    {
        public UploadServerResponse Response { get; set; }
        public string Error { get; set; }
    }

    public class UploadServerResponse
    {
        public string Upload_Url { get; set; }
    }

    public class UploadPhotoInfo
    {
        public string Error { get; set; }
        public int Server { get; set; }
        public string Hash { get; set; }
        public PhotoInfo Photo { get; set; }
    }

    public class PhotoInfo
    {
        public string Photo { get; set; }
    }
}