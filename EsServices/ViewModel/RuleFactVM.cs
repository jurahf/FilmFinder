using EsServices.ViewModel.Common;

namespace EsServices.ViewModel
{
    public class RuleFactVM : BaseViewModel
    {
        public int Order { get; set; }
        public FactVM Fact { get; set; } = null!;
    }
}
