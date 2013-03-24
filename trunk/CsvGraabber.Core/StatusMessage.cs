using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core {
    /// <summary>
    /// Delegate used to log status messages.
    /// </summary>
    public delegate void StatusChangedEventHandler(StatusMessage message);

    /// <summary>
    /// Makes logging progress during import/export operations easier
    /// </summary>
    public class StatusMessage {
        /// <summary>
        /// The message type
        /// </summary>
        public enum StatusMessageType {
            Error,
            Success,
            Status,
            Other,
            Style1,
            Style2
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets whether the message is an error.
        /// </summary>
        /// <value><c>true</c> if the message is an error, otherwise <c>false</c>.</value>
        public StatusMessageType MessageType { get; set; }
        /// <summary>
        /// Gets or sets whether this <see cref="StatusMessage"/> is a multipart message. <c>true</c> if this message should be treated as a single message, 
        /// <c>false</c> if it's one of a number of messages treated as one unit. Default is <c>true</c>.
        /// </summary>
        /// <value><c>true</c> if this message is one of many, otherwise <c>false</c>.</value>
        public bool Multipart { get; set; }

        /// <summary>
        /// Initializes a new <see cref="StatusMessage"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type of the message.</param>
        public StatusMessage(string message, StatusMessageType type) {
            this.Message = message;
            this.MessageType = type;
            Multipart = false;
        }

        /// <summary>
        /// Initializes a new <see cref="StatusMessage"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="multipart">
        /// <c>false</c> if this message should be treated as a single message, 
        /// <c>true</c> if it's one of a number of messages treated as one unit. Default is <c>true</c>.
        /// </param>
        public StatusMessage(string message, StatusMessageType type, bool multipart) 
            :this(message, type) {
            this.Multipart = multipart;
        }
    }
}
