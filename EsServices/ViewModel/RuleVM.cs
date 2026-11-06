using EsServices.ViewModel.Common;
using System.Text;

namespace EsServices.ViewModel
{
    public class RuleVM : BaseViewModel
    {
        public int Order { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Reasoning { get; set; }
        public FactVM? Result { get; set; }
        public List<RuleFactVM> Conditions { get; set; } = new();

        public string Display
        {
            get
            {
                if (Conditions.Count == 0)
                    return $"{Name}: (нет посылок)";

                var sb = new StringBuilder();
                sb.Append(Name).Append(": ЕСЛИ ");
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (i > 0)
                        sb.Append(" И ");
                    sb.Append('(').Append(Conditions[i].Fact.Display).Append(')');
                }

                sb.Append(" ТОГДА ");
                sb.Append(Result?.Display ?? "(вывод не задан)");
                return sb.ToString();
            }
        }

        public override string ToString() => Display;
    }
}
