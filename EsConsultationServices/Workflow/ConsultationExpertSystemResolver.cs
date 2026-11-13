using EsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EsConsultationServices.Workflow;

/// <summary>
/// Определяет экспертную систему для новой консультации:
/// id из запроса -> id из конфигурации -> имя из конфигурации.
/// </summary>
public sealed class ConsultationExpertSystemResolver
{
    private readonly EsDbContext db;
    private readonly ConsultationOptions options;

    public ConsultationExpertSystemResolver(EsDbContext db, IOptions<ConsultationOptions> options)
    {
        this.db = db;
        this.options = options.Value;
    }

    public async Task<int> ResolveAsync(int? expertSystemId, CancellationToken cancellationToken = default)
    {
        if (expertSystemId is > 0)
        {
            await EnsureExistsAsync(expertSystemId.Value, cancellationToken);
            return expertSystemId.Value;
        }

        if (options.ExpertSystemId > 0)
        {
            await EnsureExistsAsync(options.ExpertSystemId, cancellationToken);
            return options.ExpertSystemId;
        }

        if (string.IsNullOrWhiteSpace(options.ExpertSystemName))
        {
            throw new InvalidOperationException(
                "Не задана экспертная система: укажите Consultation:ExpertSystemId " +
                "или Consultation:ExpertSystemName в конфигурации.");
        }

        var id = await db.ExpertSystems
            .AsNoTracking()
            .Where(x => x.Name == options.ExpertSystemName)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (id == null)
        {
            throw new KeyNotFoundException(
                $"Экспертная система '{options.ExpertSystemName}' не найдена.");
        }

        return id.Value;
    }

    private async Task EnsureExistsAsync(int expertSystemId, CancellationToken cancellationToken)
    {
        var exists = await db.ExpertSystems
            .AsNoTracking()
            .AnyAsync(x => x.Id == expertSystemId, cancellationToken);

        if (!exists)
            throw new KeyNotFoundException($"Экспертная система с Id {expertSystemId} не найдена.");
    }
}
