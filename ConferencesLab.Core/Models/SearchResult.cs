namespace ConferencesLab.Core.Models;

public sealed record SearchResult(
    IReadOnlyList<Conference> Items,
    int TotalScanned,
    string QueryDescription,
    TimeSpan Elapsed);
