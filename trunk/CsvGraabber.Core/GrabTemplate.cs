using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Lime49;

namespace CsvGrabber.Core
{
    public class GrabTemplate:INotifyPropertyChanged
    {
        private string _urlFormatString,
                       _name,
                       _description,
                       _userData;
        private string _templateID;
        private GrabEventArgs _grabParams;
        private Constants.GrabModes _grabMode;
        private Constants.GrabSchedules _grabSchedule;

        public string TemplateID {
            get { return _templateID; }
            set {
                _templateID = value;
                OnPropertyChanged("TemplateID");
            }
        }

        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the description, used to inform the user.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get { return _description; }
            set {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets the string to use when formatting the URL.
        /// </summary>
        /// <value>
        /// The string to use when formatting the URL.
        /// </value>
        public string UrlFormatString {
            get { return _urlFormatString; }
            set {
                _urlFormatString = value;
                OnPropertyChanged("UrlFormatString");
            }
        }

        /// <summary>
        /// Gets or sets the string to use when formatting the URL.
        /// </summary>
        /// <value>
        /// The string to use when formatting the URL.
        /// </value>
        public string UserData {
            get { return _userData; }
            set {
                _userData = value;
                OnPropertyChanged("UserData");
                if (!string.IsNullOrWhiteSpace(UrlFormatString))
                    GrabParameters.Url.Url = string.Format(UrlFormatString, value);
            }
        }

        public GrabEventArgs GrabParameters {
            get { return _grabParams; }
            set {
                _grabParams = value;
                OnPropertyChanged("GrabParameters");
            }
        }

        public Constants.GrabModes GrabMode {
            get { return _grabMode; }
            set {
                _grabMode = value;
                OnPropertyChanged("GrabMode");
            }
        }

        public Constants.GrabSchedules GrabSchedule {
            get { return _grabSchedule; }
            set {
                _grabSchedule = value;
                OnPropertyChanged("GrabSchedule");
            }
        }

        public GrabTemplate()
        {
            GrabParameters = new GrabEventArgs();
        }

        public string GetScrapeUrl()
        {
            return string.Format(UrlFormatString, UserData);
        }


        public static GrabTemplate FromXElement(XElement element) {
            try {
                GrabTemplate template = new GrabTemplate()
                {
                    TemplateID = HtmlUtils.HtmlDecode(element.Element("id").Value),
                    Name = HtmlUtils.HtmlDecode(element.Element("name").Value),
                    Description = HtmlUtils.HtmlDecode(element.Element("description").Value).Trim(),
                    GrabMode = (Constants.GrabModes)Enum.Parse(typeof(Constants.GrabModes),element.Element("grabmode").Value,true),
                    UrlFormatString = HtmlUtils.HtmlDecode(element.Element("urlformatstring").Value),
                      GrabParameters= GrabEventArgs.Deserialize(element.Element("params").Value),
                    GrabSchedule = (Constants.GrabSchedules)Enum.Parse(typeof(Constants.GrabSchedules), element.Element("grabschedule").Value, true),
                };
                return template;
            } catch { // usually NullReferenceException, but if something's invalid the recipe will be invalid
                throw new Exception("Invalid template");
            }
        }

        public virtual XElement ToXElement() {
            XElement ele = new XElement("grabtemplate",
                new XElement("id", HtmlUtils.HtmlEncode(TemplateID)),
                new XElement("name", HtmlUtils.HtmlEncode(Name)),
                new XElement("description", HtmlUtils.HtmlEncode(Description)),
                new XElement("grabmode", (int)this.GrabMode),
                new XElement("urlformatstring", HtmlUtils.HtmlEncode(UrlFormatString)),
                new XElement("params", HtmlUtils.HtmlEncode(GrabParameters.Serialize())),
                new XElement("grabschedule", (int)this.GrabSchedule)
                );
            return ele;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string strPropertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(strPropertyName));
            }
        }
    }
}
