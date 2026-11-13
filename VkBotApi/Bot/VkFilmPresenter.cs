using EsConsultationServices.Films;

namespace VkBotApi.Bot;

/// <summary>Отправляет постер (если он есть) и текстовую карточку фильма.</summary>
public sealed class VkFilmPresenter
{
    private readonly IVkMessageSender sender;
    private readonly FilmMessageFormatter formatter;
    private readonly VkKeyboardFactory keyboards;

    public VkFilmPresenter(
        IVkMessageSender sender,
        FilmMessageFormatter formatter,
        VkKeyboardFactory keyboards)
    {
        this.sender = sender;
        this.formatter = formatter;
        this.keyboards = keyboards;
    }

    public async Task SendAsync(
        long peerId,
        string sessionId,
        RecommendedFilm film,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(film.Poster))
            await sender.SendPhotoAsync(peerId, film.Poster, cancellationToken);

        await sender.SendTextAsync(
            peerId,
            formatter.Format(film),
            keyboards.CreateFilmKeyboard(sessionId, film.Id),
            cancellationToken);
    }
}
