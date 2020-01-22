using ConsultationWeb.Classes.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace ConsultationWeb.Classes
{
    public class FilmExternalService
    {
        private static string serviceUrl = "http://localhost:3178/"; // EsService/WebApi

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serviceUrl);
            // TODO: auth info
            return client;
        }

        public AdviceDto GetAdviceDetails(string key)
        {
            string query = $"api/FilmAdvice/GetAdviceDetails?key={key}";
            var client = CreateHttpClient();

            HttpResponseMessage response = client.GetAsync(query).Result;

            if (response.IsSuccessStatusCode)
            {
                var advice = response.Content.ReadAsAsync<AdviceDto>().Result;

                return advice;
            }
            else
                return null;
        }

    }
}