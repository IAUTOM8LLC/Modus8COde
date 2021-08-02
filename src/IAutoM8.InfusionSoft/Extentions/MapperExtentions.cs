using System;
using System.Xml;

namespace IAutoM8.InfusionSoft.Extentions
{
    static class MapperExtentions
    {
        internal static Mapper<T> Prop<T>(this Mapper<T> mapper, string from, Action<T, XmlNode> propertySetter)
            where T : new()
        {
            Mapper<T>.PropertyMaps.Add(from, propertySetter);
            return mapper;
        }
    }
}
