using System.Text;
using EsDb.Model.Enums;
using EsServices.ViewModel;

namespace EsServices.Services.Export
{
    /// <summary>
    /// Преобразует граф экспертной системы в Mermaid-диаграмму.
    /// Правило изображается цепочкой «вопрос → ответ → следующий вопрос → … → свойство»,
    /// а его номер и имя пишутся на стрелке к листу.
    /// </summary>
    public sealed class ExpertSystemMermaidExporter
    {
        public string Export(ExpertSystemVM expertSystem, DateTime? exportedAt = null)
        {
            var graph = BuildGraph(expertSystem);
            var date = (exportedAt ?? DateTime.Now).ToString("yyyy-MM-dd");

            var builder = new StringBuilder();

            builder.AppendLine($"%% Экспертная система: {expertSystem.Name} (Id {expertSystem.Id})");
            builder.AppendLine($"%% Дата экспорта: {date}");
            builder.AppendLine($"%% Цель консультации: {expertSystem.Target?.Name ?? "(не задана)"}");
            builder.AppendLine($"%% Правил: {expertSystem.Rules.Count}");
            builder.AppendLine("%% Вопрос — прямоугольник, ответ — овал, свойство (вывод) — шестиугольник.");
            builder.AppendLine("%% Цепочка «вопрос → ответ → ... → лист» — условия одного правила в порядке проверки.");
            builder.AppendLine("%% Подпись на стрелке к листу — номер и имя правила.");
            builder.AppendLine("%% Правила проверяются по возрастанию номера; срабатывает первое подходящее.");
            builder.AppendLine();
            builder.AppendLine("flowchart TD");
            builder.AppendLine("    classDef question fill:#FFCC00,stroke:#333333,color:#000000");
            builder.AppendLine("    classDef answer fill:#FFFFFF,stroke:#333333,color:#000000");
            builder.AppendLine("    classDef property fill:#CCE5FF,stroke:#333333,color:#000000");
            builder.AppendLine("    classDef invalid fill:#FFCCCC,stroke:#333333,color:#000000");
            builder.AppendLine();

            foreach (var node in graph.Nodes)
                builder.AppendLine($"    {FormatNode(node)}");

            if (graph.Nodes.Count > 0)
                builder.AppendLine();

            foreach (var edge in graph.Edges)
            {
                if (string.IsNullOrEmpty(edge.Label))
                    builder.AppendLine($"    {edge.From} --> {edge.To}");
                else
                    builder.AppendLine($"    {edge.From} -->|\"{EscapeLabel(edge.Label)}\"| {edge.To}");
            }

            return builder.ToString();
        }

        private static Graph BuildGraph(ExpertSystemVM expertSystem)
        {
            var graph = new Graph();
            var questions = new Dictionary<string, MermaidNode>(StringComparer.Ordinal);
            var answers = new Dictionary<string, MermaidNode>(StringComparer.Ordinal);
            var facts = new Dictionary<string, MermaidNode>(StringComparer.Ordinal);
            var edgeKeys = new HashSet<string>(StringComparer.Ordinal);

            MermaidNode? invalidResult = null;

            void AddEdge(string from, string to, string? label = null)
            {
                var key = $"{from}\u001f{to}\u001f{label}";
                if (edgeKeys.Add(key))
                    graph.Edges.Add(new MermaidEdge(from, to, label));
            }

            var rules = expertSystem.Rules
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name, StringComparer.Ordinal)
                .ThenBy(x => x.Id)
                .ToList();

            foreach (var rule in rules)
            {
                var ruleLabel = FormatRuleLabel(rule);
                var conditions = rule.Conditions
                    .Where(x => x.Fact?.Variable != null && x.Fact.DomainValue != null)
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.Fact.Id)
                    .ToList();

                if (conditions.Count == 0)
                {
                    graph.AddNode(NodeKind.Invalid, $"{ruleLabel}: нет посылок");
                    continue;
                }

                string? parent = null;

                foreach (var condition in conditions)
                {
                    var fact = condition.Fact;
                    var variable = fact.Variable;
                    var value = fact.DomainValue;

                    if (CanAsk(variable))
                    {
                        var questionKey = $"{parent ?? "ROOT"}\u001f{variable.Name}";

                        if (!questions.TryGetValue(questionKey, out var question))
                        {
                            question = graph.AddNode(NodeKind.Question, GetQuestionText(variable));
                            questions[questionKey] = question;

                            foreach (var option in variable.Domain!.Values)
                            {
                                var answerKey = $"{question.Id}\u001f{option.Value}";
                                var answer = graph.AddNode(NodeKind.Answer, option.Display);
                                answers[answerKey] = answer;
                                AddEdge(question.Id, answer.Id);
                            }
                        }

                        var factAnswerKey = $"{question.Id}\u001f{value.Value}";
                        if (!answers.TryGetValue(factAnswerKey, out var answerNode))
                        {
                            // Значение факта отсутствует в домене: показываем его отдельным ответом.
                            answerNode = graph.AddNode(NodeKind.Answer, value.Display);
                            answers[factAnswerKey] = answerNode;
                            AddEdge(question.Id, answerNode.Id);
                        }

                        if (parent != null)
                            AddEdge(parent, question.Id);

                        parent = answerNode.Id;
                    }
                    else
                    {
                        var factNode = GetOrCreateFact(fact);
                        if (parent != null)
                            AddEdge(parent, factNode.Id);

                        parent = factNode.Id;
                    }
                }

                if (rule.Result == null)
                {
                    invalidResult ??= graph.AddNode(NodeKind.Invalid, "Вывод не задан");
                    AddEdge(parent!, invalidResult.Id, ruleLabel);
                    continue;
                }

                var resultNode = GetOrCreateFact(rule.Result);
                AddEdge(parent!, resultNode.Id, ruleLabel);
            }

            return graph;

            MermaidNode GetOrCreateFact(FactVM fact)
            {
                var key = $"{fact.Variable.Name}\u001f{fact.DomainValue.Value}";
                if (facts.TryGetValue(key, out var existed))
                    return existed;

                var node = graph.AddNode(NodeKind.Property, fact.Display);
                facts[key] = node;
                return node;
            }
        }

        private static bool CanAsk(VariableVM variable)
        {
            return variable.Type == VariableType.Queriable
                && variable.Domain != null
                && variable.Domain.Values.Count > 0;
        }

        private static string GetQuestionText(VariableVM variable)
        {
            return string.IsNullOrWhiteSpace(variable.Question)
                ? variable.Name
                : variable.Question;
        }

        private static string FormatRuleLabel(RuleVM rule)
        {
            var name = string.IsNullOrWhiteSpace(rule.Name) ? "(без имени)" : rule.Name.Trim();
            return $"{rule.Order + 1}. {name}";
        }

        private static string FormatNode(MermaidNode node)
        {
            var label = EscapeLabel(node.Label);
            return node.Kind switch
            {
                NodeKind.Question => $"{node.Id}[\"{label}\"]:::question",
                NodeKind.Answer => $"{node.Id}([\"{label}\"]):::answer",
                NodeKind.Property => $"{node.Id}{{{{\"{label}\"}}}}:::property",
                _ => $"{node.Id}{{{{\"{label}\"}}}}:::invalid"
            };
        }

        private static string EscapeLabel(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var normalized = text
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            var builder = new StringBuilder(normalized.Length + 16);
            foreach (var symbol in normalized)
            {
                switch (symbol)
                {
                    case '#':
                        builder.Append("#35;");
                        break;
                    case '&':
                        builder.Append("#amp;");
                        break;
                    case '"':
                        builder.Append("#quot;");
                        break;
                    case '<':
                        builder.Append("#lt;");
                        break;
                    case '>':
                        builder.Append("#gt;");
                        break;
                    case '\n':
                        builder.Append("<br/>");
                        break;
                    default:
                        builder.Append(symbol);
                        break;
                }
            }

            return builder.ToString();
        }

        private enum NodeKind
        {
            Question,
            Answer,
            Property,
            Invalid
        }

        private sealed class MermaidNode
        {
            public MermaidNode(string id, NodeKind kind, string label)
            {
                Id = id;
                Kind = kind;
                Label = label;
            }

            public string Id { get; }
            public NodeKind Kind { get; }
            public string Label { get; }
        }

        private sealed class MermaidEdge
        {
            public MermaidEdge(string from, string to, string? label)
            {
                From = from;
                To = to;
                Label = label;
            }

            public string From { get; }
            public string To { get; }
            public string? Label { get; }
        }

        private sealed class Graph
        {
            private int questionCounter;
            private int answerCounter;
            private int propertyCounter;
            private int invalidCounter;

            public List<MermaidNode> Nodes { get; } = new();
            public List<MermaidEdge> Edges { get; } = new();

            public MermaidNode AddNode(NodeKind kind, string label)
            {
                var id = kind switch
                {
                    NodeKind.Question => $"Q{++questionCounter}",
                    NodeKind.Answer => $"A{++answerCounter}",
                    NodeKind.Property => $"P{++propertyCounter}",
                    _ => $"X{++invalidCounter}"
                };

                var node = new MermaidNode(id, kind, label);
                Nodes.Add(node);
                return node;
            }
        }
    }
}
