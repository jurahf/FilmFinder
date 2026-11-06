namespace EsDb.Repositories
{
    public class ExpertSystemListRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DomainCount { get; set; }
        public int VariableCount { get; set; }
        public int RuleCount { get; set; }
    }
}
