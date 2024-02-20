using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;

namespace Server.Controllers
{
    public class TestController : ApiController
    {
        [HttpGet]
        [ActionName("Echo")]
        public string Echo(string text)
        {
            return text;
        }

        [HttpGet]
        [ActionName("Test")]
        public string Test()
        {
            try
            {
                Task<string> str = GetStrAsync();

                /// TODO: Код для проверки 
                DBWork db = new DBWork();
                var film = db.GetFromDatabase<Film>(x => x.Id == 5).FirstOrDefault();
                return WebUtility.HtmlDecode(film.Description);

                return "все ок";
            }
            catch (Exception ex)
            {
                return ex.Message + " " + (ex?.InnerException?.Message ?? "");
            }
        }


        private async Task<string> GetStrAsync()
        {
            await Task.Delay(5000);
            return "after sleep";
        }



    }
}
