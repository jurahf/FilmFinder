namespace TgBotWClient.Contracts;

/// <summary>Разобранная команда: имя и аргументы без потери регистра.</summary>
public sealed class TgCommandRequest
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> Arguments { get; init; } = Array.Empty<string>();

    public string? GetArgument(int index)
    {
        return index >= 0 && index < Arguments.Count ? Arguments[index] : null;
    }
}
