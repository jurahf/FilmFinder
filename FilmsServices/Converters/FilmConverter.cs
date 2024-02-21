using FilmDb.Model;
using FilmsServices.Converters.Common;
using FilmsServices.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Converters
{
    public class FilmConverter : IEntityConverter<Film, FilmVM>
    {
        private readonly IEntityConverter<Actor, ActorVM> actorConverter;
        private readonly IEntityConverter<Country, CountryVM> countryConverter;
        private readonly IEntityConverter<Genre, GenreVM> genreConverter;
        private readonly IEntityConverter<Producer, ProducerVM> producerConverter;
        private readonly IEntityConverter<CustomProperty, CustomPropertyVM> customPropConverter;


        public FilmConverter()
        {
            actorConverter = new DefaultConverter<Actor, ActorVM>();
            countryConverter = new DefaultConverter<Country, CountryVM>();
            genreConverter = new DefaultConverter<Genre, GenreVM>();
            producerConverter = new DefaultConverter<Producer, ProducerVM>();
            customPropConverter = new DefaultConverter<CustomProperty, CustomPropertyVM>();
        }


        public Film ConvertToDb(FilmVM viewModel)
        {
            // TODO: проверить, как будут сохраняться связи 
            // чтобы без дублей
            // желательно не пересоздавать все (возможно, тогда надо читать film из базы и сравнивать)


            var film = new Film()
            {
                Id = viewModel.Id,
                Description = viewModel.Description,
                KinopoiskId = viewModel.KinopoiskId,
                Link = viewModel.Link,
                Name = viewModel.Name,
                Poster = viewModel.Poster,
                Rating = viewModel.Rating,
                Slogan = viewModel.Slogan,
                Year = viewModel.Year,
            };

            film.ActorFilm = viewModel.Actors.Select(x =>
                new ActorFilm()
                {
                    Actor = actorConverter.ConvertToDb(x),
                    Film = film
                }).ToHashSet();

            film.CountryFilm = viewModel.Countries.Select(x =>
                new CountryFilm()
                {
                    Country = countryConverter.ConvertToDb(x),
                    Film = film
                }).ToHashSet();

            film.GenreFilm = viewModel.Genres.Select(x =>
                new GenreFilm()
                {
                    Genre = genreConverter.ConvertToDb(x),
                    Film = film
                }).ToHashSet();

            film.ProducerFilm = viewModel.Producers.Select(x =>
                new ProducerFilm()
                {
                    Producer = producerConverter.ConvertToDb(x),
                    Film = film
                }).ToHashSet();

            film.FilmCustomProperty = viewModel.CustomProperties
                .Select(x => new FilmCustomProperty()
                {
                    Value = x.Value,
                    Film = film,
                    CustomProperty = customPropConverter.ConvertToDb(x),
                }).ToHashSet();

            return film;    
        }

        public FilmVM ConvertToVm(Film database)
        {
            return new FilmVM()
            {
                Id = database.Id,
                Description = database.Description,
                Year = database.Year,
                Slogan = database.Slogan,
                Rating = database.Rating,
                Poster = database.Poster,
                Name = database.Name,
                Link = database.Link,
                KinopoiskId = database.KinopoiskId,
                Actors = database.ActorFilm.Select(x => actorConverter.ConvertToVm(x.Actor)).ToList(),
                Countries = database.CountryFilm.Select(x => countryConverter.ConvertToVm(x.Country)).ToList(),
                Genres = database.GenreFilm.Select(x => genreConverter.ConvertToVm(x.Genre)).ToList(),
                Producers = database.ProducerFilm.Select(x => producerConverter.ConvertToVm(x.Producer)).ToList(),
                CustomProperties = database.FilmCustomProperty.Select(x =>
                    {
                        var res = customPropConverter.ConvertToVm(x.CustomProperty);
                        res.Value = x.Value;
                        return res;
                    }).ToList(),
            };
        }
    }
}
