using FilmsServices.ViewModel.Common;

namespace FilmsServices.ViewModel
{
    public class AdviceVM : BaseViewModel
    {
        public AdviceVM()
        {
            Films = new HashSet<AdviceFilmVM>();
            CustomProperties = new HashSet<CustomPropertyVM>();
            Key = Guid.NewGuid();
        }

        public Guid Key { get; set; }


        public virtual ICollection<AdviceFilmVM> Films { get; set; }

        public virtual ICollection<CustomPropertyVM> CustomProperties { get; set; }


        /// <summary>
        /// Строка для списка советов: перечисление фильмов.
        /// </summary>
        public string FilmsStr =>
            string.Join("; ", Films
                .Select(x => x.Film?.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x)));


        /// <summary>
        /// Строка для списка советов: перечисление свойств со значениями.
        /// </summary>
        public string TagStr =>
            string.Join("; ", CustomProperties
                .Select(x => $"{(x.Value < 0 ? "НЕ " : string.Empty)}{x.Name} ({x.Value})"));


        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
