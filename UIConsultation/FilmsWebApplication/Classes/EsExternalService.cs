using FilmsWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace FilmsWebApplication.Classes
{
    public class EsExternalService
    {
        private static string serviceUrl = "http://localhost:3178/"; // ЭС/WebApi

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serviceUrl);
            // TODO: auth info
            return client;
        }

        public List<VariableDto> GetQuestions()
        {
            EsParameters parameters = new EsParameters();
            string query = $"api/ExpertSystem/GetQueriedVariables?esName={parameters.FileName}";
            var client = CreateHttpClient();

            HttpResponseMessage response = client.GetAsync(query).Result;

            if (response.IsSuccessStatusCode)
            {
                var vars = response.Content.ReadAsAsync<List<VariableDto>>().Result;

                return vars;
            }
            else
                return null;
        }

        public ConsultResultDto GetConsult(List<EsVariables> answers)
        {
            EsParameters parameters = new EsParameters();
            parameters.VarValues.AddRange(answers);

            ConsultResultDto result = null;
            if (parameters.VarValues.Any())
            {
                string json = new JavaScriptSerializer().Serialize(parameters);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpClient client = CreateHttpClient();

                HttpResponseMessage response = client.PostAsync("/api/expertSystem/GoConsult", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsAsync<ConsultResultDto>().Result;
                }
            }

            return result;
        }


    }
}