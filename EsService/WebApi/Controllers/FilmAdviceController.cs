using ExpertSystemDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebApi.Classes;

namespace WebApi.Controllers
{
    // TODO: вообще-то ему не место в сервисе ЭС

    public class FilmAdviceController : ApiController
    {
        [HttpGet]
        [ActionName("GetAdvicePreview")]
        public AdvicePreviewDto GetAdvicePreview(string key)
        {
            Guid guid;
            if (!Guid.TryParse(key, out guid))
                return null;

            IDataWork db = new DBWork(); // TODO: DI
            Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

            if (advice == null)
                return null;

            return new AdvicePreviewDto(advice);
        }

        [HttpGet]
        [ActionName("GetAdviceDetails")]
        public AdviceDto GetAdviceDetails(string key)
        {
            Guid guid;
            if (!Guid.TryParse(key, out guid))
                return null;

            IDataWork db = new DBWork(); // TODO: DI
            Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

            if (advice == null)
                return null;

            return new AdviceDto(advice);
        }


        [HttpGet]
        [ActionName("GetFilmDetails")]
        public FilmDto GetFilmDetails(int id)
        {
            IDataWork db = new DBWork(); // TODO: DI
            Film film = db.GetFromDatabase<Film>(x => x.Id == id).FirstOrDefault();

            if (film == null)
                return null;

            return new FilmDto(film);
        }


    }
}