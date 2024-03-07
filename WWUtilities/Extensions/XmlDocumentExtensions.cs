using System.Diagnostics;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class XmlDocumentExtensions
    {
        // This function has been replaced by a modified XMLBuilder.ToString() call.
        ///// <summary>
        ///// Formats the provided XML so it's indented and humanly-readable.
        ///// </summary>
        ///// <param name="inputXml">The input XML to format.</param>
        ///// <returns></returns>
        //public static string ToFormattedXML(this XmlDocument xml)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(builder){Encoding = Encoding.UTF8}))
        //    {
        //        writer.Formatting = Formatting.Indented;
        //        xml.Save(writer);
        //    }

        //    return builder.ToString();
        //}

    }
}
