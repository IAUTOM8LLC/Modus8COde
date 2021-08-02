using System;
using System.Xml;

namespace IAutoM8.InfusionSoft.Extentions
{
    static class XmlWriterBasicExtentions
    {
        internal static XmlWriter Method(this XmlWriter xmlWriter, string method)
        {
            xmlWriter.WriteElementString("methodName", method);
            return xmlWriter;
        }

        #region Value
        internal static XmlWriter Value(this XmlWriter xmlWriter, string value)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteElementString("string", value);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Value(this XmlWriter xmlWriter, int value)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteElementString("int", value.ToString());
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Value(this XmlWriter xmlWriter, bool value)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteElementString("boolean", value ? "1" : "0");
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Value(this XmlWriter xmlWriter, double value)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteElementString("double", value.ToString());
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Value(this XmlWriter xmlWriter, int[] values)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteStartElement("array");
                {
                    xmlWriter.WriteStartElement("data");
                    {
                        foreach (var value in values)
                        {
                            xmlWriter.Value(value);
                        }
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Value(this XmlWriter xmlWriter, string[] values)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteStartElement("array");
                {
                    xmlWriter.WriteStartElement("data");
                    {
                        foreach (var value in values)
                        {
                            xmlWriter.Value(value);
                        }
                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Value(this XmlWriter xmlWriter, Action<XmlWriter> valueDataGenerator)
        {
            xmlWriter.WriteStartElement("value");
            {
                xmlWriter.WriteStartElement("struct");
                valueDataGenerator(xmlWriter);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }
        #endregion

        #region Param
        internal static XmlWriter Param(this XmlWriter xmlWriter, string value)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(value);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Param(this XmlWriter xmlWriter, int value)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(value);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Param(this XmlWriter xmlWriter, bool value)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(value);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Param(this XmlWriter xmlWriter, double value)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(value);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Param(this XmlWriter xmlWriter, int[] values)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(values);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Param(this XmlWriter xmlWriter, string[] values)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(values);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Param(this XmlWriter xmlWriter, Action<XmlWriter> valueDataGenerator)
        {
            xmlWriter.WriteStartElement("param");
            {
                xmlWriter.Value(valueDataGenerator);
            }
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        #endregion

        #region Params
        internal static XmlWriter Params(this XmlWriter xmlWriter, Action<XmlWriter> parameters)
        {
            xmlWriter.WriteStartElement("params");
            parameters(xmlWriter);
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }
        #endregion

        #region Member
        internal static XmlWriter Member(this XmlWriter xmlWriter, string name, string value)
        {
            xmlWriter.WriteStartElement("member");
            xmlWriter.WriteElementString("name", name);
            xmlWriter.Value(value);
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Member(this XmlWriter xmlWriter, string name, int value)
        {
            xmlWriter.WriteStartElement("member");
            xmlWriter.WriteElementString("name", name);
            xmlWriter.Value(value);
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Member(this XmlWriter xmlWriter, string name, bool value)
        {
            xmlWriter.WriteStartElement("member");
            xmlWriter.WriteElementString("name", name);
            xmlWriter.Value(value);
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }

        internal static XmlWriter Member(this XmlWriter xmlWriter, string name, double value)
        {
            xmlWriter.WriteStartElement("member");
            xmlWriter.WriteElementString("name", name);
            xmlWriter.Value(value);
            xmlWriter.WriteEndElement();
            return xmlWriter;
        }
        #endregion
    }
}
