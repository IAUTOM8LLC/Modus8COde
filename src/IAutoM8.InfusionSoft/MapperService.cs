using IAutoM8.InfusionSoft.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;

namespace IAutoM8.InfusionSoft
{
    class MapperService : IMapperService
    {
        public T Map<T>(string value) where T : new()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(value);
            return MapToObject<T>(xmlDoc.GetElementsByTagName("member"));
        }

        public T MapToStruct<T>(string value) where T : struct
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(value);
            var result = new StructMapper<T>();
            foreach (XmlNode map in xmlDoc.GetElementsByTagName("param"))
            {
                Action<StructMapper<T>, XmlNode> action = null;
                if (Mapper<StructMapper<T>>.PropertyMaps.TryGetValue(string.Empty, out action))
                    action(result, map);
            }
            return result.Property;
        }

        public List<T> MapToList<T>(string value) where T : new()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(value);
            var result = new List<T>();
            foreach (XmlNode dataNode in xmlDoc.GetElementsByTagName("data"))
            {
                foreach (XmlNode itemNode in dataNode.SelectNodes("value"))
                {
                    result.Add(MapToObject<T>(itemNode.SelectNodes("struct/member")));
                }
            }
            return result;
        }

        private T MapToObject<T>(XmlNodeList members) where T : new()
        {
            var result = new T();
            foreach (XmlNode map in members)
            {
                Action<T, XmlNode> action = null;
                if (Mapper<T>.PropertyMaps.TryGetValue(map.FirstChild.InnerText, out action))
                    action(result, map);
            }
            return result;
        }
    }
}
