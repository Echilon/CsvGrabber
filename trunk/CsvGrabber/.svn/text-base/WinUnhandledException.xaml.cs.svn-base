using System;
using System.Windows;
using CsvGrabber.Core;

namespace CsvGrabber
{
    /// <summary>
    /// A generic dialog which allows sending feedback.
    /// </summary>
    public partial class WinUnhandledException : Window {
        private readonly Exception exception;

        /// <summary>
        /// Initializes a new <see cref="WinUnhandledException"/>.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public WinUnhandledException(Exception ex) {
            InitializeComponent();
            this.exception = ex;
            this.txtDisplayMessage.Text = "-------Message-------\n" + exception.Message +
                                          "\n\n-------StackTrace-------\n" + exception.StackTrace +
                                          "\n\n-------Inner Exception-------\n" + exception.InnerException +
                                          "\n\n-------Help Link-------\n" + exception.HelpLink;
        }

        private void SendReport(object sender, RoutedEventArgs e) {
            string message = string.Format("mailto:{0}?subject={1}&body={2}",
                                                       "bugs@lime49.com",
                                                       string.Format("CsvGrabber Error Report (v{0})", Constants.GetVersion()),
                                                       "-------Message-------%0D%0A%0D%0A" + exception.Message +
                                                       "%0D%0A%0D%0A%0D%0A%0D%0A-------StackTrace-------%0D%0A%0D%0A" + exception.StackTrace +
                                                       "%0D%0A%0D%0A%0D%0A%0D%0A-------Inner Exception-------%0D%0A%0D%0A" + exception.InnerException +
                                                       "%0D%0A%0D%0A%0D%0A%0D%0A-------Help Link-------%0D%0A%0D%0A" + exception.HelpLink).Substring(0,2000);
            try {
                System.Diagnostics.Process.Start(message);
                Close();
            } catch { }
        }

        private void CloseWindow(object sender, RoutedEventArgs e) {
            Close();
        }

        private void lnk_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            System.Diagnostics.Process.Start(e.ToString());
        }
    }
}
