using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace ww.Utilities.Extensions
{
	[DebuggerStepThrough]
	public static class XElementExtensions
	{
		/// <summary>
		/// Returns the value of the first decendant node that matches nodeName. Returns an empty string if the node was not found.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="nodeName"></param>
		/// <returns></returns>
		public static string GetDescendantValue(this XElement element, XName nodeName)
		{
			XElement target = element.Descendants(nodeName).FirstOrDefault(); // Returns a null if nodeName isn't found.
			if (target != null)
				return target.Value;
			else
				return "";
		}
		public static string GetDecendantValue(this XDocument document, XName nodeName)
		{
			XElement target = document.Descendants(nodeName).FirstOrDefault(); // Returns a null if nodeName isn't found.
			if (target != null)
				return target.Value;
			else
				return "";
		}

		public static int GetDescendantValueAsInt(this XElement element, XName nodeName, int defaultValue = 0)
		{
			int value;
			string valueString = element.GetDescendantValue(nodeName);
			if (int.TryParse(valueString, out value))
				return value;
			else
				return defaultValue;
		}

		public static decimal GetDescendantValueAsDecimal(this XElement element, XName nodeName, decimal defaultValue = 0)
		{
			decimal value;
			string valueString = element.GetDescendantValue(nodeName);
			if (decimal.TryParse(valueString, out value))
				return value;
			else
				return defaultValue;
		}
		
        /// <summary>
        /// Get first child element by tag name, not including schema values (e.g. doesn't have to be a full XName).
        /// </summary>
        /// <param name="name">string name of the tag type to find.</param>
        /// <returns></returns>
        public static XElement getElementByTagName(this XElement e, string name)
        {
            var elements = e.Elements().Where(x => x.Name.ToString().Contains(name));
            if (elements.Any())
                return elements.ElementAt(0);
            return null;
        }

        /// <summary>
        /// Get child elements by tag name, not including schema values (e.g. doesn't have to be a full XName).
        /// </summary>
        /// <param name="name">string name of the tag type to find.</param>
        /// <returns></returns>
        public static IEnumerable<XElement> getElementsByTagName(this XElement e, string name)
        {
            var elements = e.Elements().Where(x => x.Name.ToString().Contains(name));
            if (elements.Any())
                return elements;
            return null;
        }
	}
}
