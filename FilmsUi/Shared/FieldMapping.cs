namespace FilmsUi.Shared
{
    public class FieldMapping<T>
    {
        public FieldMapping(int order, string caption, Func<T, string> getVal)
        {
            this.Order = order;
            this.Caption = caption;
            this.GetVal = getVal;
        }

        public int Order { get; set; }

        public string Caption { get; set; }

        public Func<T, string> GetVal { get; set; }
    }
}
