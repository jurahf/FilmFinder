using EsConsultationServices.Films;

namespace TgBotWClient.Bot;

/// <summary>Отправляет постер (если он есть) и текстовую карточку фильма.</summary>
public sealed class TgFilmPresenter
{
    private readonly ITgMessageSender sender;
    private readonly FilmMessageFormatter formatter;
    private readonly TgKeyboardFactory keyboards;

    public TgFilmPresenter(
        ITgMessageSender sender,
        FilmMessageFormatter formatter,
        TgKeyboardFactory keyboards)
    {
        this.sender = sender;
        this.formatter = formatter;
        this.keyboards = keyboards;
    }

    public async Task SendAsync(
        long chatId,
        string sessionId,
        RecommendedFilm film,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(film.Poster))
            await sender.SendPhotoAsync(chatId, film.Poster, cancellationToken: cancellationToken);

        await sender.SendTextAsync(
            chatId,
            formatter.Format(film),
            keyboards.CreateFilmKeyboard(),
            cancellationToken);
    }
}
