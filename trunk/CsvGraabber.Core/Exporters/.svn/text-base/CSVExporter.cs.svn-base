using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                        writer.Write(response.RawResponse + ",");
                    foreach (List<string> regexMatch in response.ParsedResponse) {
                        writer.WriteLine(regexMatch.Aggregate(new StringBuilder(), (buffer, capture) => buffer.Length == 0 ? buffer.Append(capture) : buffer.Append(",").Append(capture), buffer => buffer.ToString()));
                    }
            }
        }
    }
}
