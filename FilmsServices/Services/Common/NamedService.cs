using CommonRepositories;
using FilmsServices.Converters.Common;
using FilmsServices.Validators.Common;
using FilmsServices.ViewModel.Common;

namespace FilmsServices.Services.Common
{
    /// <summary>
    /// Сервис справочника с поиском по имени.
    /// </summary>
    public interface INamedService<VM> : IService<VM> where VM : BaseViewModel
    {
        Task<List<VM>> SearchByNameAsync(string query, int limit, int page);

        Task<int> GetCountByNameAsync(string query);
    }


    public class NamedService<DB, VM> : BaseSevice<DB, VM>, INamedService<VM>
        where DB : BaseEntity, new()
        where VM : BaseViewModel
    {
        private readonly INamedRepository<DB> namedRepository;

        public NamedService(
            IRepository<DB> repository,
            IEntityConverter<DB, VM> converter,
            IValidator<VM> validator)
            : base(repository, converter, validator)
        {
            namedRepository = repository as INamedRepository<DB>
                ?? throw new ArgumentException(
                    $"Репозиторий {repository.GetType().Name} не поддерживает поиск по имени",
                    nameof(repository));
        }


        public async Task<List<VM>> SearchByNameAsync(string query, int limit, int page)
        {
            List<DB> fromDB = await namedRepository.SearchByNameAsync(query, limit, page);
            return fromDB
                .Select(d => converter.ConvertToVm(d))
                .ToList();
        }


        public async Task<int> GetCountByNameAsync(string query)
        {
            return await namedRepository.GetCountByNameAsync(query);
        }
    }
}
