using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core.Exporters
{
    public abstract class IExporter
    {
        /// <summary>
        /// Gets or sets the name of the file to which to save.
        /// </summary>
        /// <value>The name of the file of the file to which to save.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets whether the response from the specified URL should be written to the output file.
        /// </summary>
        /// <value><c>true</c> if the response from the specified URL should be written to the output file, otherwise <c>false</c>.</value>
        public bool IncludeRawResponse { get; set; }

        /// <summary>
        /// Gets or sets whether extra whitespace is trimmed for each field. This replaces double line breaks with single line breaks (\r\n\r\n -> \r\n).
        /// </summary>
        /// <value>
        ///   <c>true</c> if extra whitespace is trimmed for each field, otherwise <c>false</c>.
        /// </value>
        public bool TrimExtraWhitespace { get; set; }

        /// <summary>
        /// Saves the specified result set.
        /// </summary>
        /// <param name="response">The result set to save.</param>
        public abstract void Save(GrabResponse response);

        public IExporter() {
            FileName = string.Empty;
        }
    }
}
