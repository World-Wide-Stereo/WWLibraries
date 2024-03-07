using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ww.Utilities
{
	public class XmlBuilder : XmlDocument
	{
		private readonly Tag _outerNode;
		//public readonly string XmlNamespace;
		//private Dictionary<string, string> XmlNamespace;
		private readonly XmlNamespaceManager namespaceManager;
		private bool _isValid = true;
		private string _validationErrors;
		public string validationErrors
		{
			get { return _validationErrors.Replace(" in namespace '" + this.NamespaceURI + "'", ""); }
		}
		public Tag FirstTag
		{
			get { return _outerNode; }
		}
		//public override string OuterXml
		//{
		//	get
		//	{
		//		return base.OuterXml.Replace("’","'").Replace("™", "").Replace("•", "-");
		//	}
		//}

		public XmlBuilder(string OuterNode) : this(OuterNode, true)
		{
		    namespaceManager = new XmlNamespaceManager(this.NameTable);
		}
		//public XmlBuilder(string OuterNode, string ns) : this(OuterNode, ns, true) { }
		public XmlBuilder(string OuterNode, bool CreateDeclaration, bool standalone = false)
		{
            namespaceManager = new XmlNamespaceManager(this.NameTable);

            if ((CreateDeclaration))
            {
                base.AppendChild(base.CreateXmlDeclaration("1.0", System.Text.Encoding.UTF8.WebName, standalone ? "yes" : ""));
            }
			_outerNode = CreateElement(OuterNode);
            base.AppendChild(_outerNode);
		}

	    public XmlBuilder(string OuterNode, string defaultNamespace) : this(OuterNode, true, defaultNamespace){}
        public XmlBuilder(string OuterNode, bool CreateDeclaration, string defaultNamespace)
        {
            namespaceManager = new XmlNamespaceManager(this.NameTable);
            namespaceManager.AddNamespace("", defaultNamespace);

            if ((CreateDeclaration))
            {
                base.AppendChild(base.CreateXmlDeclaration("1.0", System.Text.Encoding.UTF8.WebName, null));
            }
            _outerNode = CreateElement(OuterNode);
            base.AppendChild(_outerNode);
        }

		protected new Tag CreateElement(string qualifiedName)
		{
			if (qualifiedName.Contains(":"))
			{
				string[] name = qualifiedName.Split(':');
				return new Tag(name[0], name[1], this);
			}
			else
			{
				return new Tag(qualifiedName, this);
			}

		}

        // // This is removed because adding a root namespace after the root element has been written messes things up.
        //public void AddNamespace(string name)
        //{
        //    //this.DocumentElement.SetAttribute("xmlns", name);
        //    //this.XmlNamespace.Add("", name);
        //    namespaceManager.AddNamespace("", name);
        //}

		public void AddNamespace(string key, string value, bool useXSI = true)
		{
			if (useXSI)
			{
				//this.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
				//this.DocumentElement.SetAttribute(key, "http://www.w3.org/2001/XMLSchema-instance", value);
                namespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                namespaceManager.AddNamespace("xsi:" + key, value);
			}
			else
			{
				//this.DocumentElement.SetAttribute("xmlns:" + key, value);
			    namespaceManager.AddNamespace(key, value);
			}
			//this.XmlNamespace.Add(key, value);
		}
		public void AddRootAttribute(string key, string value)
		{
			this.DocumentElement.SetAttribute(key, value);
		}
        //public void AddRootAttribute(string ns, string key, string value)
        //{
        //    this.DocumentElement.SetAttribute(key, XmlNamespace[ns], value);
        //}

		public Tag AddElement(XmlElement node, string name, string value)
		{
			Tag element = this.CreateElement(name);
			element.InnerText = value;
			node.AppendChild(element);
			return element;
		}

		public Tag AddElement(XmlElement node, string name)
		{
			Tag element = this.CreateElement(name);
			node.AppendChild(element);
			return element;
		}

        public bool isValidXML(string xmlSchemaFile)
        {
            _isValid = true;
            XmlSchemaSet schemas = new XmlSchemaSet();
            if (this.namespaceManager.LookupNamespace("").Length > 0)
                schemas.Add(this.namespaceManager.LookupNamespace(""), xmlSchemaFile);
            else
                schemas.Add(XmlSchema.Read(new StreamReader(xmlSchemaFile), null));
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schemas;
            settings.ValidationEventHandler += ValidationHandler;
            XmlReader xml = XmlReader.Create(new StringReader(this.OuterXml), settings);

            try
            {

                while ((xml.Read()))
                {
                }
            }
            catch (Exception ex)
            {
                _isValid = false;
                _validationErrors += ex.Message + "\n";
            }
            return _isValid;

            //			XmlSchemaSet schemas = new XmlSchemaSet();
            //	schemas.Add("http://www.example.com/WPGSchedule.xsd", SchemaPath);
            //	XmlReaderSettings settings = new XmlReaderSettings(); 
            //	settings.ValidationType = ValidationType.Schema;
            //	settings.Schemas = schemas;
            //	settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

            //	XmlReader xml = XmlReader.Create(SchedulePath, settings);
            //	//XmlReader xmlForValidation = XmlReader.Create(SchedulePath, settings); // We do this second one for validation purposes.


            //Try
            //	{
            //		//while (xmlForValidation.Read()) ;
            //return true;
        }

		private void ValidationHandler(object sender, ValidationEventArgs e)
		{
			_isValid = false;
			_validationErrors += e.Message + "\n";
		}

        public override string ToString()
        {
            return ToString(false);
        }
        public string ToString(bool format)
        {
            if (format)
            {
                return ToString(Encoding.UTF8);
            }
            else
            {
                return this.OuterXml;
            }
        }
        public string ToString(Encoding encoding)
        {
            using (var stringBuilder = new StringWriterWithEncoding(encoding))
            {


                var settings = new XmlWriterSettings
                                   {
                                       Indent = true,
                                       Encoding = encoding,
                                       NamespaceHandling = NamespaceHandling.Default,
                                   };
                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    this.Save(xmlWriter);
                }

                return stringBuilder.ToString();
            }
        }

	    public class Tag : XmlElement
		{
			private readonly XmlBuilder _builder;
			internal Tag(string Name, XmlBuilder builder) : base("", Name, builder.namespaceManager.DefaultNamespace, builder)
			{
				_builder = builder;
			}
            internal Tag(string prefix, string Name, XmlBuilder builder) : base(prefix, Name, builder.namespaceManager.LookupNamespace(prefix), builder)
            {
                _builder = builder;
            }
			public Tag AddTag(string name, string value)
			{
				return _builder.AddElement(this, name, value);
			}
			public Tag AddTag(string name, bool value)
			{
				return _builder.AddElement(this, name, value.ToString().ToLower());
			}
			public Tag AddTag(string name, object value)
			{
				return _builder.AddElement(this, name, value.ToString());
			}

			public Tag AddTag(string name)
			{
				return _builder.AddElement(this, name);
			}

			public Tag AddTag(string name, IDictionary<string, string> attributes)
			{
				var tag = _builder.AddElement(this, name);
				foreach (var attr in attributes)
				{
					tag.SetAttribute(attr.Key, attr.Value);
				}
				return tag;
			}

			public Tag AddTagsFromXPath(string xpath)
			{
				// Grab the next tag name in the XPath or return parent if empty.
				string[] tagNames = xpath.Trim('/').Split('/');
				string nextTagInXPath = tagNames.First();
				if (String.IsNullOrEmpty(nextTagInXPath)) return this;

				// Get or create the tag from the name.
				var tag = (Tag)this.SelectSingleNode(nextTagInXPath) ?? this.AddTag(nextTagInXPath);

				// Rejoin the remainder of the array as an XPath expression and recurse.
				string remainingXPath = String.Join("/", tagNames.Skip(1).ToArray());
				return tag.AddTagsFromXPath(remainingXPath);
			}
			public Tag AddTagsWithAttributesFromXPath(string xpath, string xmlNamespace = null)
			{
				// Grab the next tag name in the XPath or return parent if empty.
				string[] tagNames = xpath.Trim('/').Split('/');
				string nextTagInXPath = tagNames.First();
				if (String.IsNullOrEmpty(nextTagInXPath)) return this;

				// Grab the attributes from the next tag in the XPath.
				string attributeName = null;
				string attributeValue = null;
				string nextTagWithoutAttribute = nextTagInXPath;
				if (nextTagInXPath.Contains("[@"))
				{
					int indexOfAttritbuteName = nextTagInXPath.IndexOf("[@") + 2;
					int indexOfAttributeValue = nextTagInXPath.IndexOf("='") + 2;
					attributeName = nextTagInXPath.Substring(indexOfAttritbuteName, indexOfAttributeValue - 2 - indexOfAttritbuteName);
					attributeValue = nextTagInXPath.Substring(indexOfAttributeValue, nextTagInXPath.Length - indexOfAttributeValue - 2);
					nextTagWithoutAttribute = nextTagInXPath.Substring(0, indexOfAttritbuteName - 2);
				}

				// Get or create the tag from the name.
				var nsmgr = new XmlNamespaceManager(this.OwnerDocument.NameTable);
				if (xmlNamespace != null)
				{
					nsmgr.AddNamespace("ns", xmlNamespace);
					nextTagInXPath = "ns:" + nextTagInXPath;
				}
				var tag = (Tag)this.SelectSingleNode(nextTagInXPath, nsmgr);
				if (tag == null)
				{
					tag = this.AddTag(nextTagWithoutAttribute);
					if (attributeName != null) tag.SetAttribute(attributeName, attributeValue);
					if (xmlNamespace != null) tag.SetAttribute("xmlns", xmlNamespace);
				}

				// Rejoin the remainder of the array as an XPath expression and recurse.
				string remainingXPath = String.Join("/", tagNames.Skip(1).ToArray());
				return tag.AddTagsWithAttributesFromXPath(remainingXPath, xmlNamespace);
			}
		}
	}
}