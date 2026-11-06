using EsServices.ViewModel.Common;

namespace EsServices.ViewModel
{
    public class ExpertSystemVM : BaseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public VariableVM? Target { get; set; }

        public List<DomainVM> Domains { get; set; } = new();
        public List<VariableVM> Variables { get; set; } = new();
        public List<RuleVM> Rules { get; set; } = new();
        public List<FactVM> Facts { get; set; } = new();
    }
}
