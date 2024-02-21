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
        public Film ConvertToDb(FilmVM viewModel)
        {
            return new Film()
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
                // TODO: связи
            };
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
                // TODO: связи
            };
        }
    }
}
