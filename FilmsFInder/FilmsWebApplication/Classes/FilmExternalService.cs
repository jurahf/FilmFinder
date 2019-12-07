using FilmsWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace FilmsWebApplication.Classes
{
    public class FilmExternalService
    {
        private static string serviceUrl = "http://localhost:3175/"; // FilmsWebApi

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serviceUrl);
            // TODO: auth info
            return client;
        }

        public Film GetFilm(int id)
        {
            string query = $"api/Films/GetFilm?id={id}";
            var client = CreateHttpClient();

            HttpResponseMessage response = client.GetAsync(query).Result;

            if (response.IsSuccessStatusCode)
            {
                var film = response.Content.ReadAsAsync<Film>().Result;

                return film;
            }
            else
                return null;
        }
    }
}