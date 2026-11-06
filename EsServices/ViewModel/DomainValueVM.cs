using EsServices.ViewModel.Common;

namespace EsServices.ViewModel
{
    public class DomainValueVM : BaseViewModel
    {
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Отображаемое значение. Для домена #Советы — читаемое описание совета.
        /// </summary>
        public string Display { get; set; } = string.Empty;
    }
}
