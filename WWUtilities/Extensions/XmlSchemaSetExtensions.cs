using System.Diagnostics;
using System.Xml.Schema;

namespace ww.Utilities.Extensions
{
    /// <summary>
    /// Description of XmlSchemaSetExtensions.
    /// </summary>
    [DebuggerStepThrough]
    public static class XmlSchemaSetExtensions
    {
        /// <summary>
        /// Get the first (default) XmlSchema from this XmlSchemaSet.
        /// </summary>
        /// <returns>The first (default) XmlSchema, or null if the XmlSchemaSet is empty.</returns>
        public static XmlSchema getFirstSchema(this XmlSchemaSet set)
        {
            foreach (XmlSchema schema in set.Schemas())
                return schema;
            return null;
        }
    }
}
