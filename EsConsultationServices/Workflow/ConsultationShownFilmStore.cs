using EsDb;
using EsDb.Model;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationServices.Workflow;

/// <summary>
/// Хранит, какие фильмы уже показаны в консультации, чтобы «другой ответ»
/// не повторялся после перезапуска и при нескольких экземплярах приложения.
/// </summary>
public sealed class ConsultationShownFilmStore
{
    private readonly EsDbContext db;

    public ConsultationShownFilmStore(EsDbContext db)
    {
        this.db = db;
    }

    public async Task<IReadOnlyList<int>> GetIdsAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.ConsultationShownFilms
            .AsNoTracking()
            .Where(x => x.Consultation.Session.SessionId == sessionId)
            .OrderBy(x => x.Order)
            .Select(x => x.FilmId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Регистрирует показ фильма. Возвращает false, если параллельный запрос
    /// уже записал этот фильм или занял порядковый номер.
    /// </summary>
    public async Task<bool> TryAddAsync(
        string sessionId,
        int filmId,
        CancellationToken cancellationToken = default)
    {
        var consultationId = await db.Consultations
            .Where(x => x.Session.SessionId == sessionId)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (consultationId == null)
            throw new KeyNotFoundException($"Консультация с идентификатором сессии '{sessionId}' не найдена.");

        var nextOrder = await db.ConsultationShownFilms
            .Where(x => x.ConsultationId == consultationId.Value)
            .MaxAsync(x => (int?)x.Order, cancellationToken) ?? -1;

        var entry = db.ConsultationShownFilms.Add(new ConsultationShownFilm
        {
            ConsultationId = consultationId.Value,
            FilmId = filmId,
            Order = nextOrder + 1,
            ShownDate = DateTime.Now
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // Уникальный индекс (ConsultationId, FilmId) или (ConsultationId, Order).
            // Снимаем добавленную строку, чтобы повторный выбор начался с чистого состояния.
            entry.State = EntityState.Detached;
            return false;
        }
    }
}
