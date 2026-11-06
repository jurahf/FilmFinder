using EsServices.ViewModel.Common;

namespace EsServices.ViewModel
{
    public class FactVM : BaseViewModel
    {
        public VariableVM Variable { get; set; } = null!;
        public DomainValueVM DomainValue { get; set; } = null!;

        public string Display => $"{Variable.Name} = {DomainValue.Display}";

        public override string ToString() => Display;
    }
}
