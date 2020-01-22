using ConsultationWeb.Classes.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace ConsultationWeb.Classes
{
    public class ExpertSystemExternalService
    {
        private static string serviceUrl = "http://localhost:3178/"; // EsService/WebApi

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serviceUrl);
            // TODO: auth info
            return client;
        }

        public QuestionOrResultDto StartConsult()
        {
            string query = $"api/FilmExpert/StartConsult";
            var client = CreateHttpClient();

            HttpResponseMessage response = client.GetAsync(query).Result;

            if (response.IsSuccessStatusCode)
            {
                var vars = response.Content.ReadAsAsync<QuestionOrResultDto>().Result;

                return vars;
            }
            else
                return null;
        }

        public QuestionOrResultDto SetAnswerAndNext(string sessionId, string varName, string varValue)
        {
            AnswerEsArgs args = new AnswerEsArgs()
            {
                SessionId = sessionId,
                VarValue = new EsVariableValueArgs()
                {
                    Variable = varName,
                    Value = varValue
                }
            };

            string json = new JavaScriptSerializer().Serialize(args);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpClient client = CreateHttpClient();

            HttpResponseMessage response = client.PostAsync("/api/FilmExpert/SetAnswerAndNext", content).Result;

            QuestionOrResultDto result = null;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsAsync<QuestionOrResultDto>().Result;
            }

            return result;
        }





    }


    public class AnswerEsArgs
    {
        public string SessionId { get; set; }
        public EsVariableValueArgs VarValue { get; set; }
    }

    public class EsVariableValueArgs
    {
        public string Variable { get; set; }
        public string Value { get; set; }
    }

}