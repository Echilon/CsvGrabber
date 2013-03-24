using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.DAL {
    /// <summary>
    /// Configuration manager for CSV Grabber data layer
    /// </summary>
    public static class DalConfigurationManager {
        internal static string DatabasePath { get; set; }
        internal static bool LogToDatabase { get; set; }
        internal static string FilePath { get; set; }
        internal static bool LogToFile { get; set; }

        public static void Configure(IDalConfiguration configuration) {
            DatabasePath = configuration.DatabasePath;
            LogToDatabase = configuration.LogToDatabase;
            FilePath = configuration.FilePath;
            LogToFile = configuration.LogToFile;
        }
    }

    /// <summary>
    /// Contains database configuration data.
    /// </summary>
    public interface IDalConfiguration {
        string DatabasePath { get; set; }
        bool LogToDatabase { get; set; }
        string FilePath { get; set; }
        bool LogToFile { get; set; }
    }
}
