using EsDb.Model.Enums;
using EsServices.ViewModel;

namespace EsServices.Services
{
    /// <summary>
    /// Операции редактирования графа ЭС, которые должны поддерживать ссылочную целостность
    /// ещё до сохранения в БД.
    /// </summary>
    public static class ExpertSystemEditor
    {
        public static void DeleteVariable(ExpertSystemVM expertSystem, VariableVM variable)
        {
            expertSystem.Variables.Remove(variable);

            if (ReferenceEquals(expertSystem.Target, variable))
                expertSystem.Target = null;

            var rulesToDelete = new List<RuleVM>();
            foreach (var rule in expertSystem.Rules)
            {
                if (rule.Result?.Variable == variable)
                    rule.Result = null;

                rule.Conditions.RemoveAll(x => x.Fact.Variable == variable);

                if (rule.Conditions.Count == 0)
                    rulesToDelete.Add(rule);
            }

            foreach (var rule in rulesToDelete)
                expertSystem.Rules.Remove(rule);

            expertSystem.Facts.RemoveAll(x => x.Variable == variable);
        }

        public static void ChangeVariableType(ExpertSystemVM expertSystem, VariableVM variable, VariableType type)
        {
            variable.Type = type;

            if (type == VariableType.Queriable)
            {
                foreach (var rule in expertSystem.Rules)
                {
                    if (rule.Result?.Variable == variable)
                        rule.Result = null;
                }
            }
        }

        public static void DeleteDomain(ExpertSystemVM expertSystem, DomainVM domain)
        {
            expertSystem.Domains.Remove(domain);

            foreach (var variable in expertSystem.Variables.Where(x => x.Domain == domain))
                variable.Domain = null;

            RemoveFactsByDomainValues(expertSystem, domain.Values);
        }

        public static void DeleteDomainValue(ExpertSystemVM expertSystem, DomainVM domain, DomainValueVM value)
        {
            domain.Values.Remove(value);
            RemoveFactsByDomainValues(expertSystem, new[] { value });
        }

        public static void DeleteRule(ExpertSystemVM expertSystem, RuleVM rule)
        {
            expertSystem.Rules.Remove(rule);
            RemoveUnusedFacts(expertSystem);
        }

        public static void Reorder<T>(IList<T> list, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= list.Count || newIndex < 0 || newIndex >= list.Count || oldIndex == newIndex)
                return;

            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }

        private static void RemoveFactsByDomainValues(ExpertSystemVM expertSystem, IEnumerable<DomainValueVM> values)
        {
            var valueSet = new HashSet<DomainValueVM>(values);
            if (valueSet.Count == 0)
                return;

            var rulesToDelete = new List<RuleVM>();
            foreach (var rule in expertSystem.Rules)
            {
                if (rule.Result != null && valueSet.Contains(rule.Result.DomainValue))
                    rule.Result = null;

                rule.Conditions.RemoveAll(x => valueSet.Contains(x.Fact.DomainValue));

                if (rule.Conditions.Count == 0)
                    rulesToDelete.Add(rule);
            }

            foreach (var rule in rulesToDelete)
                expertSystem.Rules.Remove(rule);

            expertSystem.Facts.RemoveAll(x => valueSet.Contains(x.DomainValue));
        }

        private static void RemoveUnusedFacts(ExpertSystemVM expertSystem)
        {
            var used = new HashSet<FactVM>();

            foreach (var rule in expertSystem.Rules)
            {
                foreach (var condition in rule.Conditions)
                    used.Add(condition.Fact);

                if (rule.Result != null)
                    used.Add(rule.Result);
            }

            expertSystem.Facts.RemoveAll(x => !used.Contains(x));
        }
    }
}
