namespace FilmsServices.Exceptions
{
    /// <summary>
    /// Кинопоиск вернул антибот-заглушку вместо страницы фильма.
    /// </summary>
    public class KinopoiskBlockedException : Exception
    {
        public KinopoiskBlockedException()
            : base("Кинопоиск вернул страницу проверки на робота вместо страницы фильма. " +
                   "Попробуйте позже или откройте ссылку в браузере.")
        {
        }

        public KinopoiskBlockedException(string message)
            : base(message)
        {
        }

        public KinopoiskBlockedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
