namespace FilmsServices.Exceptions
{
    /// <summary>
    /// Фильм с таким названием и годом уже есть в базе.
    /// </summary>
    public class FilmAlreadyExistsException : Exception
    {
        public int ExistingFilmId { get; }

        public FilmAlreadyExistsException(int existingFilmId, string name, int year)
            : base($"Фильм «{name}» ({year}) уже есть в базе.")
        {
            ExistingFilmId = existingFilmId;
        }
    }
}
