using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;


namespace ww.Utilities
{
	[XmlRoot("dictionary")]
	public class SerializableStringDictionary : Dictionary<string, string>, IXmlSerializable
	{
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				string key = reader.Name;
                if (reader.IsEmptyElement)
                {
                    reader.Read();
                    this.Add(key, "");
                }
                else
                {
                    reader.ReadStartElement();
                    string value = reader.Value;
                    reader.Read();
                    reader.ReadEndElement();
                    this.Add(key, value);
                }

			}
			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			foreach (string key in this.Keys)
			{
				writer.WriteStartElement(key);
				writer.WriteValue(this[key]);
				writer.WriteEndElement();
			}
		}
	}
}