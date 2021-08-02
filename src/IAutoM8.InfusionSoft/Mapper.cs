using System;
using System.Collections.Generic;
using System.Xml;

namespace IAutoM8.InfusionSoft
{
    class Mapper<T> where T : new()
    {
        private static Lazy<Dictionary<string, Action<T, XmlNode>>> _propertyMaps
            = new Lazy<Dictionary<string, Action<T, XmlNode>>>(new Dictionary<string, Action<T, XmlNode>>());
        internal static Dictionary<string, Action<T, XmlNode>> PropertyMaps => _propertyMaps.Value;
    }

    class StructMapper<T> where T : struct
    {
        public T Property { get; set; }
    }
}
