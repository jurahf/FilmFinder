using CommonRepositories;
using EsDb.Model;

namespace EsDb.Repositories
{
    public interface IExpertSystemRepository : IRepository<ExpertSystem>
    {
        Task<ExpertSystem?> GetGraphAsync(int id);
        Task<ExpertSystem?> GetGraphByNameAsync(string name);
        Task<List<ExpertSystemListRow>> GetListAsync();
        Task<List<Fact>> GetFactsAsync(int expertSystemId);
    }
}
