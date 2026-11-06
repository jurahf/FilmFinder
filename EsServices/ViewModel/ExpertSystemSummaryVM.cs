namespace EsServices.ViewModel
{
    public class ExpertSystemSummaryVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DomainCount { get; set; }
        public int VariableCount { get; set; }
        public int RuleCount { get; set; }
        public bool IsAutoVersion => Name.Contains("_auto_", StringComparison.OrdinalIgnoreCase);
    }
}
