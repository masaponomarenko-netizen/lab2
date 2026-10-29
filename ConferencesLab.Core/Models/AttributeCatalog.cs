namespace ConferencesLab.Core.Models;





public sealed class AttributeCatalog
{
    public AttributeCatalog(
        int conferenceCount,
        IReadOnlyList<string> attributeNames,
        IReadOnlyDictionary<string, IReadOnlyList<string>> valuesByAttribute)
    {
        ConferenceCount = conferenceCount;
        AttributeNames = attributeNames;
        ValuesByAttribute = valuesByAttribute;
    }

    public int ConferenceCount { get; }

    public IReadOnlyList<string> AttributeNames { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ValuesByAttribute { get; }

    public IReadOnlyList<string> GetValues(string attributeName) =>
        ValuesByAttribute.TryGetValue(attributeName, out var values) ? values : Array.Empty<string>();
}
