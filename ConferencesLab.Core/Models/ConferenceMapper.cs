using System.Xml;
using System.Xml.Linq;

namespace ConferencesLab.Core.Models;




public static class ConferenceMapper
{
    public static Conference FromXmlElement(XmlElement element)
    {
        var attributes = element.Attributes
            .Cast<XmlAttribute>()
            .Select(attribute => new KeyValuePair<string, string>(attribute.Name, attribute.Value))
            .ToList();

        var fields = new List<KeyValuePair<string, string>>();
        CollectFields(element, string.Empty, fields);

        return new Conference(attributes, fields);
    }

    public static Conference FromXElement(XElement element)
    {
        var attributes = element.Attributes()
            .Select(attribute => new KeyValuePair<string, string>(attribute.Name.LocalName, attribute.Value))
            .ToList();

        var fields = new List<KeyValuePair<string, string>>();
        CollectFields(element, string.Empty, fields);

        return new Conference(attributes, fields);
    }

    private static void CollectFields(XmlElement parent, string prefix, List<KeyValuePair<string, string>> fields)
    {
        foreach (var child in parent.ChildNodes.OfType<XmlElement>())
        {
            var path = string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}.{child.Name}";

            foreach (var attribute in child.Attributes.Cast<XmlAttribute>())
            {
                fields.Add(new KeyValuePair<string, string>($"{path}/@{attribute.Name}", attribute.Value));
            }

            var hasElementChildren = child.ChildNodes.OfType<XmlElement>().Any();
            if (hasElementChildren)
            {
                CollectFields(child, path, fields);
                continue;
            }

            var text = child.InnerText.Trim();
            if (text.Length > 0)
            {
                fields.Add(new KeyValuePair<string, string>(path, text));
            }
        }
    }

    private static void CollectFields(XElement parent, string prefix, List<KeyValuePair<string, string>> fields)
    {
        foreach (var child in parent.Elements())
        {
            var path = string.IsNullOrEmpty(prefix) ? child.Name.LocalName : $"{prefix}.{child.Name.LocalName}";

            foreach (var attribute in child.Attributes())
            {
                fields.Add(new KeyValuePair<string, string>($"{path}/@{attribute.Name.LocalName}", attribute.Value));
            }

            if (child.Elements().Any())
            {
                CollectFields(child, path, fields);
                continue;
            }

            var text = child.Value.Trim();
            if (text.Length > 0)
            {
                fields.Add(new KeyValuePair<string, string>(path, text));
            }
        }
    }
}
