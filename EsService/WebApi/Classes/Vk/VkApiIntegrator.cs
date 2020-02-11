using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace WebApi.Classes.Vk
{
    public class VkApiIntegrator
    {
        private const string token = "89dbe5916cca655ab346340a0c1df410568552a6b3d248155ae6ba64a1d1d5b8a2fb662a98817937070c3";


        public void SendMessage(int peer_id, string msg, Keyboard keyboard, string attachment = "")
        {
            // https://api.vk.com/method/METHOD_NAME?PARAMETERS&access_token=ACCESS_TOKEN&v=V
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.vk.com/");

                int rand_id = GenerateRandId();

                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("access_token", token));
                keyValues.Add(new KeyValuePair<string, string>("random_id", rand_id.ToString()));
                if (!string.IsNullOrEmpty(msg))
                    keyValues.Add(new KeyValuePair<string, string>("message", msg));
                keyValues.Add(new KeyValuePair<string, string>("peer_id", peer_id.ToString()));
                keyValues.Add(new KeyValuePair<string, string>("v", "5.90"));
                if (keyboard != null)
                    keyValues.Add(new KeyValuePair<string, string>("keyboard", keyboard.GetJson()));
                if (!string.IsNullOrEmpty(attachment))
                    keyValues.Add(new KeyValuePair<string, string>("attachment", attachment));
                var content = new FormUrlEncodedContent(keyValues);

                HttpResponseMessage response = client.PostAsync("method/messages.send", content).Result;
            }
        }


        public string PreparePictureAndGetName(string picUrl, int peerId)
        {
            // 0. Скачать картинку
            byte[] picBytes = DownloadPicture(picUrl);
            string fileName = picUrl.Substring(picUrl.LastIndexOf('/') + 1);
            // 1. Получить адрес сервера 
            string serverUrl = GetMessageUploadPhotoUrl(peerId);

            if (string.IsNullOrEmpty(serverUrl))
                return "";

            // 2. Отправить картинку по этому адресу
            UploadPhotoInfo info = UploadPhoto(serverUrl, fileName, picBytes);

            if (info == null 
                || !string.IsNullOrEmpty(info.Error) 
                || string.IsNullOrEmpty(info.Photo.Trim('[', ']', '"')))
            {
                return "";
            }

            // 3. Сохранить картинку
            SavePhotoResultResponse photoResult = SaveMessagePhoto(info);

            if (photoResult == null)
                return "";

            // 4. Вернуть имя картинки
            return $"photo{photoResult.Owner_id}_{photoResult.Id}";
        }

        private byte[] DownloadPicture(string picUrl)
        {
            byte[] bytes;
            using (var client = new WebClient())
            {
                bytes = client.DownloadData(picUrl);
            }

            return bytes;
        }

        private string GetMessageUploadPhotoUrl(int peer_id)
        {
            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri("");

                HttpResponseMessage response =
                    client.GetAsync($"https://api.vk.com/method/photos.getMessagesUploadServer?access_token={token}&v=5.90&peer_id={peer_id}").Result;

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

        private UploadPhotoInfo UploadPhoto(string url, string fileName, byte[] fileBytes)
        {
            string jsonString = new CustomMultipartSender()
                .UploadFilesToServer(new Uri(url), fileName, fileBytes, "multipart/form-data");

            UploadPhotoInfo uploadInfo = JsonConvert.DeserializeObject<UploadPhotoInfo>(jsonString);
            if (uploadInfo?.Photo == null)
                throw new Exception(uploadInfo.Error);
            else
                return uploadInfo;
        }

        private SavePhotoResultResponse SaveMessagePhoto(UploadPhotoInfo info)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.vk.com/");

                HttpResponseMessage response =
                    client.GetAsync($"method/photos.saveMessagesPhoto?access_token={token}&v=5.90&hash={info.Hash}&server={info.Server}&photo={info.Photo}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = response.Content.ReadAsStringAsync().Result;
                    SavePhotoResult result = JsonConvert.DeserializeObject<SavePhotoResult>(jsonContent);

                    return result.Response.FirstOrDefault();
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
        public string Photo { get; set; }
    }

    public class SavePhotoResult
    {
        public SavePhotoResultResponse[] Response { get; set; }
    }

    public class SavePhotoResultResponse
    {
        public long Id { get; set; }
        public long Owner_id { get; set; }
    }

}