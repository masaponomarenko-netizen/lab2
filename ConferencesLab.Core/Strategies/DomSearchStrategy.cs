using System.Diagnostics;
using System.Xml;
using ConferencesLab.Core.Models;

namespace ConferencesLab.Core.Strategies;





public sealed class DomSearchStrategy : IXmlSearchStrategy
{
    public string Name => "DOM API";

    public SearchResult Search(string xmlPath, SearchQuery query)
    {
        var stopwatch = Stopwatch.StartNew();

        var document = new XmlDocument { XmlResolver = null };
        document.Load(xmlPath);

        var allNodes = document.SelectNodes("//conference")?.Cast<XmlNode>().OfType<XmlElement>().ToList()
            ?? new List<XmlElement>();

        var xpath = query.BuildXPath();
        var selectedNodes = document.SelectNodes(xpath)?.Cast<XmlNode>().OfType<XmlElement>() ?? Enumerable.Empty<XmlElement>();

        var results = selectedNodes
            .Select(ConferenceMapper.FromXmlElement)
            .ToList();

        stopwatch.Stop();
        return new SearchResult(results, allNodes.Count, query.DescribeForDom(), stopwatch.Elapsed);
    }
}
