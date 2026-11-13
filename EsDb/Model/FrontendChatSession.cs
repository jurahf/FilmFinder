using CommonRepositories;

namespace EsDb.Model
{
    /// <summary>
    /// Привязка внешнего чата (фронтенда) к текущей консультации.
    /// Нужна мессенджерам, где кнопка не несёт sessionId в payload:
    /// после перезапуска сервиса консультация продолжается по этой записи.
    /// </summary>
    public class FrontendChatSession : BaseEntity
    {
        /// <summary>Ключ фронтенда, например telegram или vk.</summary>
        public string Frontend { get; set; } = string.Empty;

        /// <summary>Идентификатор чата во внешней системе.</summary>
        public string ChatId { get; set; } = string.Empty;

        /// <summary>Текущая консультация пользователя.</summary>
        public string SessionId { get; set; } = string.Empty;

        public DateTime UpdateDate { get; set; }

        public virtual Session Session { get; set; } = null!;
    }
}
