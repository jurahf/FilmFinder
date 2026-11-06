using EsDb.Model.Enums;
using EsServices.ViewModel;

namespace EsServices.Validators
{
    public class ExpertSystemValidator
    {
        public ValidationResult Validate(ExpertSystemVM expertSystem)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(expertSystem.Name))
                result.Add("Имя экспертной системы не может быть пустым.", nameof(expertSystem.Name));

            ValidateDomains(expertSystem, result);
            ValidateVariables(expertSystem, result);
            ValidateRules(expertSystem, result);
            ValidateTarget(expertSystem, result);

            return result;
        }

        private static void ValidateDomains(ExpertSystemVM expertSystem, ValidationResult result)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var domain in expertSystem.Domains)
            {
                if (string.IsNullOrWhiteSpace(domain.Name))
                {
                    result.Add("Имя домена не может быть пустым.");
                    continue;
                }

                if (!names.Add(domain.Name))
                    result.Add($"Домен с именем '{domain.Name}' уже существует.");

                var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var value in domain.Values)
                {
                    if (string.IsNullOrWhiteSpace(value.Value))
                    {
                        result.Add($"В домене '{domain.Name}' есть пустое значение.");
                        continue;
                    }

                    if (!values.Add(value.Value))
                        result.Add($"В домене '{domain.Name}' значение '{value.Value}' повторяется.");
                }
            }
        }

        private static void ValidateVariables(ExpertSystemVM expertSystem, ValidationResult result)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var variable in expertSystem.Variables)
            {
                if (string.IsNullOrWhiteSpace(variable.Name))
                {
                    result.Add("Имя переменной не может быть пустым.");
                    continue;
                }

                if (!names.Add(variable.Name))
                    result.Add($"Переменная с именем '{variable.Name}' уже существует.");
            }

            foreach (var rule in expertSystem.Rules)
            {
                if (rule.Result == null)
                    continue;

                if (rule.Result.Variable.Type == VariableType.Queriable)
                {
                    result.Add(
                        $"Правило '{rule.Name}' выводит запрашиваемую переменную '{rule.Result.Variable.Name}'. " +
                        "Запрашиваемая переменная не может быть результатом правила.");
                }
            }
        }

        private static void ValidateRules(ExpertSystemVM expertSystem, ValidationResult result)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in expertSystem.Rules)
            {
                if (string.IsNullOrWhiteSpace(rule.Name))
                    result.Add("Имя правила не может быть пустым.");
                else if (!names.Add(rule.Name))
                    result.Add($"Правило с именем '{rule.Name}' уже существует.");

                if (rule.Conditions.Count == 0)
                    result.Add($"Правило '{rule.Name}' не содержит ни одной посылки.");

                if (rule.Result == null)
                    result.Add($"У правила '{rule.Name}' не задан вывод.");

                var conditionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var condition in rule.Conditions)
                {
                    var fact = condition.Fact;
                    if (fact?.Variable == null)
                    {
                        result.Add($"В правиле '{rule.Name}' есть посылка без переменной.");
                        continue;
                    }

                    if (fact.DomainValue == null)
                    {
                        result.Add($"В правиле '{rule.Name}' у переменной '{fact.Variable.Name}' не выбрано значение.");
                        continue;
                    }

                    if (fact.Variable.Domain == null)
                    {
                        result.Add(
                            $"Переменная '{fact.Variable.Name}' используется в правиле '{rule.Name}', но у неё не задан домен.");
                    }
                    else if (!fact.Variable.Domain.Values.Any(x =>
                                 string.Equals(x.Value, fact.DomainValue.Value, StringComparison.Ordinal)))
                    {
                        result.Add(
                            $"Значение '{fact.DomainValue.Value}' отсутствует в домене переменной '{fact.Variable.Name}'.");
                    }

                    var key = $"{fact.Variable.Name}\u0000{fact.DomainValue.Value}";
                    if (!conditionKeys.Add(key))
                        result.Add($"В правиле '{rule.Name}' повторяется посылка '{fact.Display}'.");
                }

                if (rule.Result != null)
                {
                    if (rule.Result.Variable == null)
                    {
                        result.Add($"У правила '{rule.Name}' вывод без переменной.");
                    }
                    else
                    {
                        if (rule.Result.Variable.Domain == null)
                        {
                            result.Add(
                                $"Переменная вывода '{rule.Result.Variable.Name}' правила '{rule.Name}' не имеет домена.");
                        }
                        else if (rule.Result.DomainValue == null ||
                                 !rule.Result.Variable.Domain.Values.Any(x =>
                                     string.Equals(x.Value, rule.Result.DomainValue.Value, StringComparison.Ordinal)))
                        {
                            result.Add(
                                $"Значение вывода правила '{rule.Name}' отсутствует в домене переменной '{rule.Result.Variable.Name}'.");
                        }

                        if (conditionKeys.Contains($"{rule.Result.Variable.Name}\u0000{rule.Result.DomainValue?.Value}"))
                        {
                            result.Add($"Правило '{rule.Name}' выводит факт, который уже есть в посылках.");
                        }
                    }
                }
            }
        }

        private static void ValidateTarget(ExpertSystemVM expertSystem, ValidationResult result)
        {
            if (expertSystem.Target == null)
            {
                result.Add("Не выбрана цель консультации.");
                return;
            }

            if (!expertSystem.Variables.Contains(expertSystem.Target))
                result.Add("Цель консультации не входит в список переменных.");

            if (expertSystem.Target.Type == VariableType.Queriable)
                result.Add("Цель консультации не может быть запрашиваемой переменной.");
        }
    }
}
