using CommonRepositories;
using FilmsServices.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmsServices.Converters.Common
{
    public interface IEntityConverter<DB, VM>
        where DB : BaseEntity
        where VM : BaseViewModel
    {
        DB ConvertToDb(VM viewModel);

        VM ConvertToVm(DB database);
    }
}
