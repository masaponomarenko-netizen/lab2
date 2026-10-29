using System.Text;
using System.Text.RegularExpressions;

namespace ConferencesLab.Core.Models;






public sealed record SearchQuery(string? AttributeName, string? AttributeValue, string? Keyword)
{
    private static readonly Regex SafeAttributeName = new(
        @"^[A-Za-z_][A-Za-z0-9_.-]*$",
        RegexOptions.Compiled);

    
    private const string UpperCase =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

    private const string LowerCase =
        "abcdefghijklmnopqrstuvwxyzабвгдеёєжзиіїйклмнопрстуфхцчшщъыьэюя";

    public bool HasAttributeFilter =>
        !string.IsNullOrWhiteSpace(AttributeName) && AttributeValue is not null;

    public bool HasKeyword => !string.IsNullOrWhiteSpace(Keyword);

    public string NormalizedKeyword => (Keyword ?? string.Empty).Trim();

    
    
    
    
    public string BuildXPath()
    {
        var builder = new StringBuilder("//conference");

        if (HasAttributeFilter)
        {
            var name = AttributeName!.Trim();
            if (!SafeAttributeName.IsMatch(name))
            {
                throw new ArgumentException($"Некоректне ім'я атрибута: {name}", nameof(AttributeName));
            }

            builder.Append("[@")
                .Append(name)
                .Append('=')
                .Append(ToXPathLiteral(AttributeValue!))
                .Append(']');
        }

        if (HasKeyword)
        {
            var keyword = ToXPathLiteral(NormalizedKeyword.ToLowerInvariant());

            builder.Append("[contains(translate(string(.), ")
                .Append(ToXPathLiteral(UpperCase))
                .Append(", ")
                .Append(ToXPathLiteral(LowerCase))
                .Append("), ")
                .Append(keyword)
                .Append(") or @*[contains(translate(., ")
                .Append(ToXPathLiteral(UpperCase))
                .Append(", ")
                .Append(ToXPathLiteral(LowerCase))
                .Append("), ")
                .Append(keyword)
                .Append(")] or .//@*[contains(translate(., ")
                .Append(ToXPathLiteral(UpperCase))
                .Append(", ")
                .Append(ToXPathLiteral(LowerCase))
                .Append("), ")
                .Append(keyword)
                .Append(")]]");
        }

        return builder.ToString();
    }

    public string DescribeForDom() => $"XPath: {BuildXPath()}";

    public string DescribeForLinq()
    {
        var builder = new StringBuilder("doc.Descendants(\"conference\")");

        if (HasAttributeFilter)
        {
            builder.Append($".Where(e => (string?)e.Attribute(\"{AttributeName!.Trim()}\") == \"{AttributeValue}\")");
        }

        if (HasKeyword)
        {
            builder.Append($".Where(e => SearchableText(e).Contains(\"{NormalizedKeyword}\", OrdinalIgnoreCase))");
        }

        return "LINQ to XML: " + builder;
    }

    public string DescribeForSax()
    {
        var parts = new List<string> { "XmlReader (потоковий обхід)" };

        if (HasAttributeFilter)
        {
            parts.Add($"@{AttributeName!.Trim()} == \"{AttributeValue}\"");
        }

        if (HasKeyword)
        {
            parts.Add($"keyWord.Contains(\"{NormalizedKeyword}\", OrdinalIgnoreCase)");
        }

        return "SAX: " + string.Join(" && ", parts);
    }

    public static string ToXPathLiteral(string value)
    {
        if (!value.Contains('\''))
        {
            return $"'{value}'";
        }

        if (!value.Contains('"'))
        {
            return $"\"{value}\"";
        }

        var parts = value.Split('\'');
        var builder = new StringBuilder("concat(");

        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", \"'\", ");
            }

            builder.Append('\'').Append(parts[i]).Append('\'');
        }

        return builder.Append(')').ToString();
    }
}
