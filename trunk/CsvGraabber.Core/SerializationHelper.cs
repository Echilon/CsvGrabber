using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Lime49;

namespace CsvGrabber.Core
{
    public class SerializationHelper
    {
        public static string SerializeResponse(IEnumerable<IEnumerable<string >> response)
        {
            var doc = new XDocument();
            var root = new XElement("response");
            doc.Add(root);
            foreach(var resp in response)
            {
                var eleItems = new XElement("matches");
                foreach(var match in resp)
                {
                    eleItems.Add("match", HtmlUtils.HtmlEncode(match));
                }
                root.Add(new XElement("item",eleItems));
            }
            return doc.ToString();
        }
    }
}
