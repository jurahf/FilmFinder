using CommonRepositories;
using FilmsServices.Validators.Common;
using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Services.Common
{
    public interface IService<VM>
        where VM : BaseViewModel
    {
        Task<List<VM>> GetAllAsync(int limit, int page);

        Task<List<VM>> GetAllAsync();

        Task<VM> GetByIdAsync(long id);

        Task<ValidationResult> UpdateAsync(VM entity);

        Task<ValidationResult> UpdateAllAsync(List<VM> entities);

        Task<ValidationResult> AddAsync(VM entity);

        Task DeleteAsync(long id);

        Task<int> GetCountAsync();
    }
}
