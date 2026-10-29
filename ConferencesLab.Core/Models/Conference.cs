using System.Text;

namespace ConferencesLab.Core.Models;






public sealed class Conference
{
    public Conference(
        IReadOnlyList<KeyValuePair<string, string>> attributes,
        IReadOnlyList<KeyValuePair<string, string>> fields)
    {
        Attributes = attributes;
        Fields = fields;

        SearchableText = string.Join(
            '\n',
            attributes.Select(pair => pair.Value)
                .Concat(fields.Select(pair => pair.Value)));
    }

    
    public IReadOnlyList<KeyValuePair<string, string>> Attributes { get; }

    
    
    
    
    public IReadOnlyList<KeyValuePair<string, string>> Fields { get; }

    
    public string SearchableText { get; }

    public string Title =>
        GetValue("title")
        ?? GetValue("name")
        ?? GetValue("id")
        ?? "(без назви)";

    public string? GetValue(string name)
    {
        foreach (var pair in Attributes)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }

    public string ToDisplayString(int number)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{number}. {Title}");

        foreach (var pair in Attributes)
        {
            builder.AppendLine($"   @{pair.Key} = {pair.Value}");
        }

        foreach (var pair in Fields)
        {
            builder.AppendLine($"   {pair.Key} = {pair.Value}");
        }

        return builder.ToString().TrimEnd();
    }
}
