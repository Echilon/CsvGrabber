using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;

namespace CsvGrabber.Core {
    /// <summary>
    /// Logs messages to a RichTextBox.
    /// </summary>
    public class RichTextBoxLogger : ILogProvider {
        private RichTextBox textBox;
        private Span spanContainer;

        private Style styError,
                      stySuccess, // every successful query
                      styStatus, // intermediate status messages
                      styOther,
                      sty1,
                      sty2;

        /// <summary>
        /// Initializes a new <see cref="RichTextBoxLogger"/>.
        /// </summary>
        /// <param name="textBox">The RichTextBox to which to log.</param>
        public RichTextBoxLogger(RichTextBox textBox) {
            if(textBox == null) {
                throw new ArgumentNullException("textBox", "TextBox cannot be null");
            }
            this.textBox = textBox;
            spanContainer = new Span();
            textBox.Document = new FlowDocument(new Paragraph(spanContainer));
            styError = new Style(typeof(Run));
            styError.Setters.Add(new Setter(Run.ForegroundProperty, Brushes.Red));
            stySuccess = new Style(typeof(Run));
            stySuccess.Setters.Add(new Setter(Run.ForegroundProperty, Brushes.Green));
            styStatus = new Style(typeof(Run));
            styStatus.Setters.Add(new Setter(Run.ForegroundProperty, Brushes.Black));
            styOther = new Style(typeof(Run));
            styOther.Setters.Add(new Setter(Run.ForegroundProperty, Brushes.CornflowerBlue));
            sty1 = new Style(typeof(Run));
            sty1.Setters.Add(new Setter(Run.ForegroundProperty, Brushes.Orange));
            sty2 = new Style(typeof(Run));
            sty2.Setters.Add(new Setter(Run.ForegroundProperty, Brushes.Navy));
        }

        public void Log(string message) {
            Log(message, false);
        }

        public void Log(string message, bool isError) {
            Log(new StatusMessage(message, isError ? StatusMessage.StatusMessageType.Error : StatusMessage.StatusMessageType.Status));
        }

        public void Log(string message, StatusMessage.StatusMessageType messageType) {
            Log(new StatusMessage(message, messageType));
        }

        public void Log(StatusMessage message) {
            textBox.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate {
                Run run = new Run(message.Message + (message.Multipart || message.Message.EndsWith("\n") ? "" : "\n"));
                switch(message.MessageType) {
                    case StatusMessage.StatusMessageType.Error:
                        run.Style = styError;
                        break;
                    case StatusMessage.StatusMessageType.Success:
                        run.Style = stySuccess;
                        break;
                    case StatusMessage.StatusMessageType.Status:
                        run.Style = styStatus;
                        break;
                    case StatusMessage.StatusMessageType.Other:
                        run.Style = styOther;
                        break;
                    case StatusMessage.StatusMessageType.Style1:
                        run.Style = sty1;
                        break;
                    case StatusMessage.StatusMessageType.Style2:
                        run.Style = sty2;
                        break;
                }
                spanContainer.Inlines.Add(new Span(run));
                textBox.ScrollToEnd();
            });
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        public void ClearLog() {
            spanContainer.Inlines.Clear();
        }
    }
}
