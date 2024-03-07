using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ww.Utilities
{
    [DebuggerStepThrough]
    public class XMLSerializer
    {
        #region Serialize
        /// <summary>
        /// Serializes the specified <see cref="object"/> to an XML <see cref="string"/>.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>The serialized string.</returns>
        public static string Serialize<T>(T data, Encoding encoding = null)
        {
            using (var stringWriter = encoding == null ? new StringWriter() : new StringWriterWithEncoding(encoding))
            {
                new XmlSerializer(typeof(T)).Serialize(stringWriter, data);
                return stringWriter.ToString();
            }
        }
        /// <summary>
        /// Serializes the specified <see cref="object"/> to an XML file.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <param name="file">The file to save the serialized string to.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public static void Serialize<T>(T data, string file, Encoding encoding = null)
        {
            using (var streamWriter = encoding == null ? new StreamWriter(file) : new StreamWriter(file, false, encoding))
            {
                new XmlSerializer(typeof(T)).Serialize(streamWriter, data);
            }
        }

        /// <summary>
        /// Serializes the specified <see cref="object"/> to an XML <see cref="string"/>.<para/>
        /// The XML declaration and all namespaces are omitted.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name ="omitXmlDeclaration">When true, the XML declaration that appears the beginning of the XML text is omitted.</param>
        /// <returns>The serialized string.</returns>
        public static string SerializeClean<T>(T data, Encoding encoding = null, bool omitXmlDeclaration = true)
        {
            using (var stringWriter = encoding == null ? new StringWriter() : new StringWriterWithEncoding(encoding))
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = omitXmlDeclaration }))
                {
                    new XmlSerializer(data.GetType()).Serialize(xmlWriter, data, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                    return stringWriter.ToString();
                }
            }
        }
        #endregion

        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML <see cref="string"/> into an object of the specified type.
        /// </summary>
        /// <param name="data">The string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(string data)
        {
            using (var stringReader = new StringReader(data))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(stringReader);
            }
        }
        /// <summary>
        /// Deserializes the specified <see cref="Stream"/> containing XML into an object of the specified type.
        /// </summary>
        /// <param name="stream">The stream to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(Stream stream)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
        }
        /// <summary>
        /// Deserializes the specified <see cref="XDocument"/> into an object of the specified type.
        /// </summary>
        /// <param name="doc">The XDocument to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(XDocument doc)
        {
            using (XmlReader xmlReader = doc.CreateReader())
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes the specified XML file into an object of the specified type.
        /// </summary>
        /// <param name="file">The file to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T DeserializeFile<T>(string file)
        {
            using (var streamReader = new StreamReader(file))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(streamReader);
            }
        }
        #endregion
    }
}
