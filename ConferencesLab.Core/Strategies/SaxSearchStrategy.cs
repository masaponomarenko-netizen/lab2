using System.Diagnostics;
using System.Text;
using System.Xml;
using ConferencesLab.Core.Models;

namespace ConferencesLab.Core.Strategies;




public sealed class SaxSearchStrategy : IXmlSearchStrategy
{
    public string Name => "SAX API";

    public SearchResult Search(string xmlPath, SearchQuery query)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<Conference>();
        var totalScanned = 0;

        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            DtdProcessing = DtdProcessing.Prohibit
        };

        using var reader = XmlReader.Create(xmlPath, settings);

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "conference")
            {
                continue;
            }

            var conference = ReadConference(reader);
            totalScanned++;

            if (Matches(conference, query))
            {
                results.Add(conference);
            }
        }

        stopwatch.Stop();
        return new SearchResult(results, totalScanned, query.DescribeForSax(), stopwatch.Elapsed);
    }

    private static Conference ReadConference(XmlReader reader)
    {
        var attributes = new List<KeyValuePair<string, string>>();

        if (reader.HasAttributes)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                attributes.Add(new KeyValuePair<string, string>(reader.LocalName, reader.Value));
            }

            reader.MoveToElement();
        }

        var fields = new List<KeyValuePair<string, string>>();

        if (!reader.IsEmptyElement)
        {
            var conferenceDepth = reader.Depth;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == conferenceDepth)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    ReadElement(reader, string.Empty, fields);
                }
            }
        }

        return new Conference(attributes, fields);
    }

    
    private static void ReadElement(
        XmlReader reader,
        string prefix,
        List<KeyValuePair<string, string>> fields)
    {
        var path = string.IsNullOrEmpty(prefix) ? reader.LocalName : $"{prefix}.{reader.LocalName}";

        if (reader.HasAttributes)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                fields.Add(new KeyValuePair<string, string>($"{path}/@{reader.LocalName}", reader.Value));
            }

            reader.MoveToElement();
        }

        if (reader.IsEmptyElement)
        {
            return;
        }

        var elementDepth = reader.Depth;
        var text = new StringBuilder();

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == elementDepth)
            {
                break;
            }

            if (reader.NodeType == XmlNodeType.Element)
            {
                ReadElement(reader, path, fields);
            }
            else if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA or XmlNodeType.SignificantWhitespace)
            {
                text.Append(reader.Value);
            }
        }

        var value = text.ToString().Trim();
        if (value.Length > 0)
        {
            fields.Add(new KeyValuePair<string, string>(path, value));
        }
    }

    private static bool Matches(Conference conference, SearchQuery query)
    {
        if (query.HasAttributeFilter)
        {
            var value = conference.GetValue(query.AttributeName!.Trim());
            if (!string.Equals(value, query.AttributeValue, StringComparison.Ordinal))
            {
                return false;
            }
        }

        if (query.HasKeyword &&
            !conference.SearchableText.Contains(query.NormalizedKeyword, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
