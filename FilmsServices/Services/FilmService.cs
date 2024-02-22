using CommonRepositories;
using FilmDb.Model;
using FilmDb.Repositories;
using FilmsServices.Converters.Common;
using FilmsServices.Services.Common;
using FilmsServices.Validators.Common;
using FilmsServices.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Services
{
    public interface IFilmService : IService<FilmVM>
    {
        Task<List<FilmVM>> SearchByName(string query, int limit, int page);
    }


    public class FilmService : BaseSevice<Film, FilmVM>, IFilmService
    {
        private readonly IFilmRepository repository;

        public FilmService(
            IFilmRepository repository, 
            IEntityConverter<Film, FilmVM> converter, 
            IValidator<FilmVM> validator) 
            : base(repository, converter, validator)
        {
            this.repository = repository;
        }

        public async Task<List<FilmVM>> SearchByName(string query, int limit, int page)
        {
            List<Film> fromDB = await repository.SearchByName(query, limit, page);
            return fromDB
                .Select(d => converter.ConvertToVm(d))
                .ToList();
        }
    }
}
