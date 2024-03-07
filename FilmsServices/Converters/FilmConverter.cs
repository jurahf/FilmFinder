using FilmDb.Model;
using FilmsServices.Converters.Common;
using FilmsServices.ViewModel;

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


        public Film FillDb(Film film, FilmVM viewModel)
        {
            //database.Id = viewModel.Id;
            film.Description = viewModel.Description;
            film.KinopoiskId = viewModel.KinopoiskId;
            film.Link = viewModel.Link;
            film.Name = viewModel.Name;
            film.Poster = viewModel.Poster;
            film.Rating = viewModel.Rating;
            film.Slogan = viewModel.Slogan;
            film.Year = viewModel.Year;

            FillActorFilm(film, viewModel);
            FillCountryFilm(film, viewModel);
            FillGenreFilm(film, viewModel);
            FillProducerFilm(film, viewModel);
            FillCustomPropFilm(film, viewModel);

            return film;
        }


        // TODO: Как-то это неправильно

        private void FillActorFilm(Film film, FilmVM viewModel)
        {
            // удяляем пропавшие связи
            List<ActorFilm> forDel = new List<ActorFilm>();
            foreach (var db in film.ActorFilm)
            {
                if (!viewModel.Actors.Any(x => x.Id == db.ActorId))
                {
                    forDel.Add(db);
                }
            }

            foreach (var del in forDel)
            {
                film.ActorFilm.Remove(del);
            }

            // добавляем новые связи
            foreach (var vm in viewModel.Actors)
            {
                ActorFilm? actorFilm = film.ActorFilm.FirstOrDefault(x => x.ActorId == vm.Id);

                if (actorFilm == null)
                {
                    actorFilm = new ActorFilm()
                    {
                        Film = film,
                        ActorId = vm.Id,
                        //Actor = new Actor() { Id = vm.Id }
                    };

                    film.ActorFilm.Add(actorFilm);
                }
            }
        }

        private void FillCountryFilm(Film film, FilmVM viewModel)
        {
            // удяляем пропавшие связи
            List<CountryFilm> forDel = new List<CountryFilm>();
            foreach (var db in film.CountryFilm)
            {
                if (!viewModel.Countries.Any(x => x.Id == db.CountryId))
                {
                    forDel.Add(db);
                }
            }

            foreach (var del in forDel)
            {
                film.CountryFilm.Remove(del);
            }

            // добавляем новые связи
            foreach (var vm in viewModel.Countries)
            {
                CountryFilm? countryFilm = film.CountryFilm.FirstOrDefault(x => x.CountryId == vm.Id);

                if (countryFilm == null)
                {
                    countryFilm = new CountryFilm()
                    {
                        Film = film,
                        CountryId = vm.Id,
                    };

                    film.CountryFilm.Add(countryFilm);
                }
            }
        }

        private void FillGenreFilm(Film film, FilmVM viewModel)
        {
            // удяляем пропавшие связи
            List<GenreFilm> forDel = new List<GenreFilm>();
            foreach (var db in film.GenreFilm)
            {
                if (!viewModel.Genres.Any(x => x.Id == db.GenreId))
                {
                    forDel.Add(db);
                }
            }

            foreach (var del in forDel)
            {
                film.GenreFilm.Remove(del);
            }

            // добавляем новые связи
            foreach (var vm in viewModel.Genres)
            {
                GenreFilm? genreFilm = film.GenreFilm.FirstOrDefault(x => x.GenreId == vm.Id);

                if (genreFilm == null)
                {
                    genreFilm = new GenreFilm()
                    {
                        Film = film,
                        GenreId = vm.Id,
                    };

                    film.GenreFilm.Add(genreFilm);
                }
            }
        }

        private void FillProducerFilm(Film film, FilmVM viewModel)
        {
            // удяляем пропавшие связи
            List<ProducerFilm> forDel = new List<ProducerFilm>();
            foreach (var db in film.ProducerFilm)
            {
                if (!viewModel.Producers.Any(x => x.Id == db.ProducerId))
                {
                    forDel.Add(db);
                }
            }

            foreach (var del in forDel)
            {
                film.ProducerFilm.Remove(del);
            }

            // добавляем новые связи
            foreach (var vm in viewModel.Producers)
            {
                ProducerFilm? producerFilm = film.ProducerFilm.FirstOrDefault(x => x.ProducerId == vm.Id);

                if (producerFilm == null)
                {
                    producerFilm = new ProducerFilm()
                    {
                        Film = film,
                        ProducerId = vm.Id,
                    };

                    film.ProducerFilm.Add(producerFilm);
                }
            }
        }

        private void FillCustomPropFilm(Film film, FilmVM viewModel)
        {
            // удяляем пропавшие связи
            List<FilmCustomProperty> forDel = new List<FilmCustomProperty>();
            foreach (var db in film.FilmCustomProperty)
            {
                if (!viewModel.CustomProperties.Any(x => x.Id == db.CustomPropertyId))
                {
                    forDel.Add(db);
                }
            }

            foreach (var del in forDel)
            {
                film.FilmCustomProperty.Remove(del);
            }

            // еще и редактируем связи
            foreach (var vm in viewModel.CustomProperties)
            {
                FilmCustomProperty? customPropFilm = film.FilmCustomProperty.FirstOrDefault(x => x.CustomPropertyId == vm.Id);

                if (customPropFilm != null)
                {
                    customPropFilm.Value = vm.Value;
                }
            }

            // добавляем новые связи
            foreach (var vm in viewModel.CustomProperties)
            {
                FilmCustomProperty? customPropFilm = film.FilmCustomProperty.FirstOrDefault(x => x.CustomPropertyId == vm.Id);

                if (customPropFilm == null)
                {
                    customPropFilm = new FilmCustomProperty()
                    {
                        Film = film,
                        CustomPropertyId = vm.Id,
                        Value = vm.Value,
                    };

                    film.FilmCustomProperty.Add(customPropFilm);
                }
            }
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
