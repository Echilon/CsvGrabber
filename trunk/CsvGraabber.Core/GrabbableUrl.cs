using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Lime49;

namespace CsvGrabber.Core
{
    /// <summary>
    /// A URL which can be grabbed.
    /// </summary>
    public class GrabbableUrl
    {
        public string Url { get; set; }

        public GrabbableUrl(string url) {
            this.Url = url;
        }

        /// <summary>
        /// Initializes a new <see cref="GrabbableUrl"/> from an XElement.
        /// </summary>
        /// <param name="element">The element.</param>
        public GrabbableUrl(XElement element)
        {
            if(element.Name != "url")
            //XElement urlElement = element.Element("url");
            //if (urlElement == null)
                throw new ArgumentException("Invalid URL");
            this.Url  =HtmlUtils.HtmlDecode(element.Value);
        }

        /// <summary>
        /// Converts this GrabbableUrl to an XElement.
        /// </summary>
        /// <returns>An  representing this GrabbableUrl</returns>
        public XElement ToXElement()
        {
            XElement ele = new XElement("grabbableurl",
                new XElement("url", HtmlUtils.HtmlEncode(this.Url)));
            return ele;
        }

        public override string ToString()
        {
            return Url;
        }

        public void Clone()
        {
            throw new NotImplementedException();
        }
    }
}
