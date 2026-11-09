using System.Xml.Xsl;

namespace XmlDormitoryApp;

public class XmlTransformer
{
    public void TransformToHtml(string xmlPath, string xslPath, string outputHtmlPath)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(xslPath);
        xslt.Transform(xmlPath, outputHtmlPath);
    }
}