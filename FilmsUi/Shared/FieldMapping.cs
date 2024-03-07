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

        public FieldMapping(
            int order,
            string caption,
            Func<T, string> getVal,
            bool withEdit,
            Action<T, string> setVal)
            : this(order, caption, getVal)
        {
            this.WithEdit = withEdit;
            this.SetVal = setVal;
        }

        public bool WithEdit { get; set; }

        public int Order { get; set; }

        public string Caption { get; set; }

        public Func<T, string> GetVal { get; set; }

        public Action<T, string> SetVal { get; set; }
    }
}
