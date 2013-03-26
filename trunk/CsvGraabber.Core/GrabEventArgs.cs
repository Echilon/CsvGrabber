using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Lime49;

namespace CsvGrabber.Core
{
    public class GrabEventArgs : EventArgs
    {
        public GrabbableUrl Url { get; set; }
        public Regex GrabExpression { get; set; }

        public GrabEventArgs() {
            Url = new GrabbableUrl(string.Empty);
        }

        public GrabEventArgs(GrabbableUrl urlToGrab)
            : this(){
            Url =urlToGrab;
        }

        public GrabEventArgs(GrabbableUrl urlToGrab, string grabExpression)
            : this(urlToGrab)
        {
            this.GrabExpression = new Regex (grabExpression);
        }

        public string Serialize()
        {
            XElement ele = new XElement("grabeventargs",
                new XElement("grabexpression", this.GrabExpression),
                new XElement("ignorewhitespace", this.GrabExpression.Options.HasFlag(RegexOptions.IgnorePatternWhitespace)),
                new XElement("ignorecase", this.GrabExpression.Options.HasFlag(RegexOptions.IgnoreCase)),
                new XElement("cultureinvariant", this.GrabExpression.Options.HasFlag(RegexOptions.CultureInvariant)),
                new XElement("multiline", this.GrabExpression.Options.HasFlag(RegexOptions.Multiline)),
                new XElement("singleline", this.GrabExpression.Options.HasFlag(RegexOptions.Singleline)),
                new XElement("url", this.Url));
            return ele.ToString();
        }

        public static GrabEventArgs Deserialize(string serialized) {
            XElement ele = XElement.Parse(serialized);
            RegexOptions opts=RegexOptions.None;
            bool ignoreWhiteSpace = Convert.ToBoolean(ele.Element("ignorewhitespace").Value);
            if(ignoreWhiteSpace)
                opts |= RegexOptions.IgnorePatternWhitespace;
            bool ignoreCase = Convert.ToBoolean(ele.Element("ignorecase").Value);
            if (ignoreCase)
                opts |= RegexOptions.IgnoreCase;
            bool cultureinvariant = Convert.ToBoolean(ele.Element("cultureinvariant").Value);
            if (cultureinvariant)
                opts |= RegexOptions.CultureInvariant;
            bool multiline = Convert.ToBoolean(ele.Element("multiline").Value);
            if (multiline)
                opts |= RegexOptions.Multiline;
            bool singleline = Convert.ToBoolean(ele.Element("singleline").Value);
            if (singleline)
                opts |= RegexOptions.Singleline;

            GrabEventArgs args = new GrabEventArgs()
            {
                GrabExpression = new Regex(HtmlUtils.HtmlDecode(ele.Element("grabexpression").Value),opts),
                Url = new GrabbableUrl(ele.Element("url")),
            };
            return args;
        }
    }
}
