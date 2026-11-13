using System.Net;
using System.Text;
using EsConsultationServices.Films;

namespace VkBotApi.Bot;

public sealed class FilmMessageFormatter
{
    public string Format(RecommendedFilm film)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{WebUtility.HtmlDecode(film.Name ?? string.Empty)} ({film.Year})");
        builder.AppendLine();
        builder.AppendLine("Осторожно, возможен машинный перевод!");
        builder.AppendLine();
        builder.AppendLine(WebUtility.HtmlDecode(film.Description ?? string.Empty));
        builder.AppendLine();
        builder.AppendLine($"Рейтинг IMDb: {film.Rating:N2}");
        builder.AppendLine();
        builder.AppendLine("Основываясь на мнении пользователей, этот фильм: ");
        builder.AppendLine(string.Join(
            Environment.NewLine,
            film.Tags
                .OrderByDescending(x => x.Percent)
                .Select(x => $"{x.Percent}% {x.Name}")));

        return builder.ToString();
    }
}
