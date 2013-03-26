using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lime49;

namespace CsvGrabber.Core
{
    public class Constants
    {
        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <returns>The application version.</returns>
        public static Version GetVersion()
        {
            return System.Reflection.Assembly.GetCallingAssembly().GetName().Version;
        }

        /// <summary>
        /// Gets the date the assembly was compiled.
        /// </summary>
        public static DateTime BuildDate = Lime49.Utils.RetrieveLinkerTimestamp();

        /// <summary>
        /// The connection string for SQLite. Placeholder1 = filename
        /// </summary>
        public const string SQLiteConnectionString = "Data Source={0}";

        public enum GrabModes
        {
            Regex,
            Scrape
        }

        public enum GrabSchedules
        {
            OneTime,
            Interval
        }

        public enum GrabResult
        {
            Pending,
            Success,
            Fail
        }

        public enum GrabSource {
            Url,
            File
        }
    }
}
