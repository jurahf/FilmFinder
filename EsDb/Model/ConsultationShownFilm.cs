using CommonRepositories;

namespace EsDb.Model
{
    /// <summary>
    /// Фильм, показанный пользователю в рамках консультации.
    /// Нужен, чтобы «другой ответ» не повторял уже показанные фильмы,
    /// в том числе после перезапуска API.
    /// </summary>
    public class ConsultationShownFilm : BaseEntity
    {
        /// <summary>Id фильма из FilmsDb. Между базами нет FK.</summary>
        public int FilmId { get; set; }

        /// <summary>Порядковый номер показа в консультации, начиная с 0.</summary>
        public int Order { get; set; }

        public DateTime ShownDate { get; set; }

        public int ConsultationId { get; set; }
        public virtual Consultation Consultation { get; set; } = null!;
    }
}
