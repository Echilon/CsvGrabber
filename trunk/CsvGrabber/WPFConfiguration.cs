using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvGrabber.DAL;
using CsvGrabber.Properties;
using Lime49;

namespace CsvGrabber {
    public class WPFConfiguration : IDalConfiguration{
        /// <summary>
        /// Gets or sets the path to the database file.
        /// </summary>
        /// <value>The path to the database file.</value>
        public string DatabasePath { get; set; }

        public bool LogToDatabase { get; set; }
        public string LogDbPath { get; set; }

        public string FilePath { get; set; }

        public bool LogToFile { get; set; }

        public bool TrimExtraWhitespace { get; set; }

        public bool LogRawResponse { get; set; }
        public bool AppendLogFile { get; set; }

        protected WPFConfiguration() {
        }

        internal static void LoadConfiguration() {
            var config = GetConfig();
            DalConfigurationManager.Configure(config);
        }

        public static WPFConfiguration GetConfig()
        {
            string appPath = Utils.GetApplicationPath();
            WPFConfiguration config = new WPFConfiguration()
            {
                FilePath = string.IsNullOrWhiteSpace(Settings.Default.FilePath)?System.IO.Path.Combine(appPath, "data"):Settings.Default.FilePath,
                LogToFile = Settings.Default.LogToFile,
                DatabasePath = string.IsNullOrWhiteSpace(Settings.Default.DBPath)?System.IO.Path.Combine(appPath, "data.db3"):Settings.Default.DBPath,
                LogToDatabase = Settings.Default.LogToDatabase,
                LogRawResponse = Settings.Default.LogRawResponse,
                AppendLogFile = Settings.Default.AppendLogFile,
                TrimExtraWhitespace=Settings.Default.TrimExtraWhitespace,
                LogDbPath = Settings.Default.LogDbPath,
            };
            return config;
        }
    }
}
