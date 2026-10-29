using ConferencesLab.Core.Models;
using ConferencesLab.Core.Strategies;

namespace ConferencesLab.Core.Services;





public sealed class XmlSearchContext
{
    private readonly IReadOnlyDictionary<string, IXmlSearchStrategy> _strategies;

    public XmlSearchContext(IEnumerable<IXmlSearchStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(strategy => strategy.Name, StringComparer.Ordinal);
        Names = _strategies.Keys.ToList();
    }

    public IReadOnlyList<string> Names { get; }

    public SearchResult Search(string strategyName, string xmlPath, SearchQuery query)
    {
        if (!_strategies.TryGetValue(strategyName, out var strategy))
        {
            throw new ArgumentException($"Невідома стратегія пошуку: {strategyName}", nameof(strategyName));
        }

        return strategy.Search(xmlPath, query);
    }
}
