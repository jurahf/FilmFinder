using EsConsultationApi.Contracts;
using EsConsultationServices.Films;

namespace EsConsultationApi.Mapping;

public static class FilmMapper
{
    public static FilmDto ToDto(RecommendedFilm film)
    {
        return new FilmDto
        {
            Id = film.Id,
            Name = film.Name,
            Year = film.Year,
            Rating = film.Rating,
            Description = film.Description,
            Slogan = film.Slogan,
            Poster = film.Poster,
            Link = film.Link,
            KinopoiskId = film.KinopoiskId,
            Genres = film.Genres.ToList(),
            Countries = film.Countries.ToList(),
            Actors = film.Actors.ToList(),
            Producers = film.Producers.ToList(),
            Tags = film.Tags
                .Select(x => new FilmTagDto { Name = x.Name, Percent = x.Percent })
                .ToList()
        };
    }
}
