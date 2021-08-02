using System.Xml;

namespace IAutoM8.InfusionSoft.Extentions
{
    static class XmlNodeExtensions
    {
        internal static int ToInt(this XmlNode xmlNode)
        {
            return int.Parse(xmlNode.LastChild.HasChildNodes ?
                xmlNode.LastChild.FirstChild.InnerText : xmlNode.LastChild.InnerText);
        }
        internal static double ToDouble(this XmlNode xmlNode)
        {
            return double.Parse(xmlNode.LastChild.HasChildNodes ?
                xmlNode.LastChild.FirstChild.InnerText : xmlNode.LastChild.InnerText);
        }
        internal static bool ToBool(this XmlNode xmlNode)
        {
            return xmlNode.LastChild.HasChildNodes ?
                xmlNode.LastChild.FirstChild.InnerText == "1" :
                xmlNode.LastChild.InnerText == "1";
        }
        internal static string ToStringValue(this XmlNode xmlNode)
        {
            return xmlNode.LastChild.InnerText;
        }
    }
}
