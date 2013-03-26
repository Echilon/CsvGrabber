using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvGrabber.Core.Exporters
{
    public class CSVExporter : IExporter
    {
        public bool AppendToFile { get; set; }

        /// <summary>
        /// Initializes a <see cref="CSVExporter"/>.
        /// </summary>
        /// <param name="fileName">Name of the file to which to write.</param>
        /// <param name="append"><c>true</c> to append to the existing file, <c>false</c> to create a new file.</param>
        public CSVExporter(string fileName, bool append)
            : base() {
                this.FileName = fileName;
                this.AppendToFile = append;
        }

        public override void Save(GrabResponse response) {
            using (StreamWriter writer = new StreamWriter(FileName, AppendToFile, Encoding.UTF8)) {
                    if (IncludeRawResponse)
                        writer.Write(SanitizeCsvValue(response.RawResponse) + ",");
                    foreach (List<string> regexMatch in response.ParsedResponse) {
                        writer.WriteLine(regexMatch.Aggregate(new StringBuilder(), (buffer, capture) => buffer.Length == 0 ? buffer.Append(SanitizeCsvValue(TrimWhiteSpace(capture))) : buffer.Append(",").Append(SanitizeCsvValue(TrimWhiteSpace(capture))), buffer => buffer.ToString()));
                    }
            }
        }

        private string TrimWhiteSpace(string capture) {
            if (this.TrimExtraWhitespace) {
                return capture.Replace("\r\n\r\n", "\r\n")
                    .Replace("\r\n", string.Empty)
                    .Replace("  ", " ")
                    .Replace("\t", string.Empty);
            } else {
                return capture;
            }
        }

        private static string SanitizeCsvValue(object value)
        {
            if (value == null)
                return "";
            if (value is Nullable && ((INullable)value).IsNull)
                return "";

            if (value is DateTime) {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString();

            if (output.Contains(",") || output.Contains("\""))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            return output;

        }
    }
}
