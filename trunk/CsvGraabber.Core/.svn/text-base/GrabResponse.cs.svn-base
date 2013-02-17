using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core
{
    public class GrabResponse : GrabbedJob
    {
        public string RawResponse { get; set; }

        /// <summary>
        /// Gets or sets the parsed contents of the URL. 
        /// Each item in the first list is a match of the regular expression, each item within each list is a capture in the regex.
        /// Ie:
        /// <code>
        ///     Listlt;Match&lt;Listlt;Group&gt;&gt;
        /// </code>
        /// </summary>
        /// <value>The parsed response.</value>
        public List<List<string>> ParsedResponse { get; set; }

        public byte[] BinaryResponse { get; set; }

        public GrabResponse()
        {
            ParsedResponse = new List<List<string>>();
            BinaryResponse = new byte[0];
        }
    }
}
