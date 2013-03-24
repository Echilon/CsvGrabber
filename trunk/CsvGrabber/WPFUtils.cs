using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace CsvGrabber
{
    public class WPFUtils
    {
        /// <summary>
        /// Opens the help file. This avoids messy Windows.Forms usings in every file.
        /// </summary>
        /// <param name="pageName">Name of the page at which to open the help file or <c>null</c> for the contents page.</param>
        public static void OpenHelp(string pageName)
        {
            if (pageName == null) {
                //System.Windows.Forms.Help.ShowHelp(null, "CsvGrabber.chm");
            } else {
                //System.Windows.Forms.Help.ShowHelp(null, "CsvGrabber.chm", System.Windows.Forms.HelpNavigator.Topic, pageName);
            }
        }

        /// <summary>
        /// Waits for the BackgroundWorker to complete before returning.
        /// </summary>
        public static void WaitForWorker(BackgroundWorker worker)
        {
            if (worker != null && worker.IsBusy) {
                while (worker.IsBusy) {
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate() { }));
                }
            }
        }
    }
}
