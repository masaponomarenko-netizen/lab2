using System.Diagnostics;
using System.Xml.Linq;
using ConferencesLab.Core.Models;

namespace ConferencesLab.Core.Strategies;




public sealed class LinqSearchStrategy : IXmlSearchStrategy
{
    public string Name => "LINQ to XML";

    public SearchResult Search(string xmlPath, SearchQuery query)
    {
        var stopwatch = Stopwatch.StartNew();

        var document = XDocument.Load(xmlPath, LoadOptions.None);
        var allConferences = document.Descendants("conference").ToList();

        IEnumerable<XElement> filtered = allConferences;

        if (query.HasAttributeFilter)
        {
            var attributeName = query.AttributeName!.Trim();
            var attributeValue = query.AttributeValue;

            filtered = filtered.Where(element =>
                string.Equals((string?)element.Attribute(attributeName), attributeValue, StringComparison.Ordinal));
        }

        if (query.HasKeyword)
        {
            filtered = filtered.Where(element =>
                SearchableText(element).Contains(query.NormalizedKeyword, StringComparison.OrdinalIgnoreCase));
        }

        var results = filtered
            .Select(ConferenceMapper.FromXElement)
            .ToList();

        stopwatch.Stop();
        return new SearchResult(results, allConferences.Count, query.DescribeForLinq(), stopwatch.Elapsed);
    }

    private static string SearchableText(XElement element) =>
        string.Join(
            '\n',
            element.DescendantsAndSelf().Attributes().Select(attribute => attribute.Value)
                .Concat(element.DescendantNodes().OfType<XText>().Select(text => text.Value)));
}
