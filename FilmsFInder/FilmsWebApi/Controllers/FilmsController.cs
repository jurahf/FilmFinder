using FilmsWebApi.Classes;
using FilmsWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FilmsWebApi.Controllers
{
    public class FilmsController : ApiController
    {
        private DBWork db = new DBWork();

        [HttpGet]
        [ActionName("GetFilms")]
        public List<FilmDTO> GetFilms()
        {
            return db.GetFromDatabase<Film>().Select(x => new FilmDTO(x)).ToList();
        }

        [HttpGet]
        [ActionName("GetFilm")]
        public FilmDTO GetFilms(int id)
        {
            Film film = db.GetFromDatabase<Film>(x => x.Id == id).FirstOrDefault();
            return film == null ? null : new FilmDTO(film);
        }





    }
}
