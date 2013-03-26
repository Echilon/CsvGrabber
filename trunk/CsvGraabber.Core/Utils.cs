using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Lime49;

namespace CsvGrabber.Core
{
    public class Utils
    {
        /// <summary>
        /// Find the first unique index of a string in a collection of strings. The incrementation starts at 2.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="otherNames">The other names.</param>
        /// <returns>The first unique index of a string in a collection of strings</returns>
        /// <example>
        /// Collection contains - name, name1, name2. 
        /// "name3" will be returned.
        /// </example>
        public static string GetNextAvailableName(string name, IEnumerable<string> otherNames) {
            if (!otherNames.Contains(name)) {
                return name;
            } else {
                bool gotName = false;
                string currentName = name;
                for (int i = 2; !gotName; ) {
                    currentName = name + ' ' + i;
                    if (otherNames.Contains(currentName)) {
                        i++;
                    } else {
                        name = currentName;
                        gotName = true;
                    }
                }
                return name;
            }
        }

        /// <summary>
        /// Serializes a collection of URLs.
        /// </summary>
        /// <param name="urls">The URLs.</param>
        /// <returns>An XDocument containing the URLs.</returns>
        public static string SerializeUrls(IEnumerable<GrabbableUrl> urls) {
            XDocument doc = new XDocument();
            XElement eleUrls = new XElement("urls");
            doc.Add(eleUrls);
            foreach (GrabbableUrl url in urls) {
                eleUrls.Add(url.ToXElement());
            }
            return doc.ToString();
        }

        /// <summary>
        /// Deserializes a list of URLs.
        /// </summary>
        /// <param name="serializedSavedUrls">The serialized list of URLs.</param>
        /// <returns>The GrabbableUrls parsed from the list</returns>
        public static IEnumerable<GrabbableUrl> DeserializeUrls(string serializedSavedUrls)
        {
            XDocument doc = XDocument.Parse(serializedSavedUrls);
            XElement root = doc.Element("urls");
            var urlElements = root.Elements("grabbableurl");
            var urls = urlElements.Select(e => new GrabbableUrl(e));
            return urls;
        }

        public static string SerializeList(IEnumerable<string> items)
        {
            XDocument doc = new XDocument();
            XElement eleItems = new XElement("items");
            doc.Add(eleItems);
            foreach (string itm in items) {
                eleItems.Add(new XElement("item", HtmlUtils.HtmlEncode(itm)));
            }
            return doc.ToString();
        }

        public static List<string> DeserializeList(string serializedItems)
        {
            XDocument doc = XDocument.Parse(serializedItems);
            XElement root = doc.Element("items");
            var elements = root.Elements("item");
            var urls = elements.Select(e => HtmlUtils.HtmlDecode(e.Value));
            return urls.ToList();
        }

        public static string SanitizeFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidReStr = string.Format(@"[{0}\.]*", invalidChars);
            return System.Text.RegularExpressions.Regex.Replace(name, invalidReStr, "_");
        }
    }
}