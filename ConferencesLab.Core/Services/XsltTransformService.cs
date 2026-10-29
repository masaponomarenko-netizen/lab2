using System.Xml;
using System.Xml.Xsl;

namespace ConferencesLab.Core.Services;


public sealed class XsltTransformService
{
    public string Transform(string xmlPath, string xslPath, string outputPath)
    {
        var transform = new XslCompiledTransform();
        var settings = new XsltSettings(enableDocumentFunction: false, enableScript: false);

        transform.Load(xslPath, settings, stylesheetResolver: null);

        var arguments = new XsltArgumentList();
        arguments.AddParam("generated", string.Empty, DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

        using var writer = XmlWriter.Create(outputPath, transform.OutputSettings);
        transform.Transform(xmlPath, arguments, writer);

        return outputPath;
    }
}
