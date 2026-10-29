using ConferencesLab.Core.Models;

namespace ConferencesLab.Core.Strategies;





public interface IXmlSearchStrategy
{
    string Name { get; }

    SearchResult Search(string xmlPath, SearchQuery query);
}
