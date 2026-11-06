using EsDb.Model.Enums;
using EsServices.ViewModel.Common;

namespace EsServices.ViewModel
{
    public class VariableVM : BaseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Question { get; set; }
        public string? Reasoning { get; set; }
        public VariableType Type { get; set; } = VariableType.Queriable;

        public DomainVM? Domain { get; set; }

        public string TypeDisplay => Type switch
        {
            VariableType.Queriable => "Запрашиваемая",
            VariableType.Detective => "Выводимая",
            VariableType.QuerriableDetective => "Выводимо-запрашиваемая",
            _ => Type.ToString()
        };

        public override string ToString() => Name;
    }
}
