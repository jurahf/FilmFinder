using FilmsServices.ViewModel.Common;

namespace FilmsServices.ViewModel
{
    /// <summary>
    /// Фильм в совете вместе со степенью соответствия.
    /// </summary>
    public class AdviceFilmVM : BaseViewModel
    {
        public FilmVM Film { get; set; } = new FilmVM();

        public int Value { get; set; }


        public int FilmId => Film?.Id ?? 0;

        public string Name => Film?.Name ?? string.Empty;


        public override string ToString()
        {
            return $"{Name} ({Value})";
        }
    }
}
