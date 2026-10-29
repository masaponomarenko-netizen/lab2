using System.Xml;
using ConferencesLab.Core.Models;

namespace ConferencesLab.Core.Services;





public sealed class AttributeCatalogService
{
    public AttributeCatalog Load(string xmlPath)
    {
        var values = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        var conferenceCount = 0;

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

            conferenceCount++;

            if (!reader.HasAttributes)
            {
                continue;
            }

            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (!values.TryGetValue(reader.LocalName, out var set))
                {
                    set = new SortedSet<string>(StringComparer.Ordinal);
                    values[reader.LocalName] = set;
                }

                set.Add(reader.Value);
            }

            reader.MoveToElement();
        }

        if (conferenceCount == 0)
        {
            throw new InvalidDataException("У файлі не знайдено жодного вузла <conference>.");
        }

        var names = values.Keys.OrderBy(name => name, StringComparer.Ordinal).ToList();
        var valuesByAttribute = names.ToDictionary(
            name => name,
            name => (IReadOnlyList<string>)values[name].ToList(),
            StringComparer.Ordinal);

        return new AttributeCatalog(conferenceCount, names, valuesByAttribute);
    }
}
