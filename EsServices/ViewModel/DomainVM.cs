using EsServices.ViewModel.Common;

namespace EsServices.ViewModel
{
    public class DomainVM : BaseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public List<DomainValueVM> Values { get; set; } = new();
    }
}
