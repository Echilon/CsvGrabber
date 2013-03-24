using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CsvGrabber.Core;
using CsvGrabber.Properties;
using Lime49.UI;

namespace CsvGrabber
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WinAddFromTemplate : Window
    {
        /// <summary>
        /// Gets or sets the template being used.
        /// </summary>
        /// <value>The grab template being used.</value>
        public GrabTemplate GrabTemplate
        {
            get { return (DataContext as GrabTemplate); }
            set { DataContext = value;}
        }

        public ScheduledGrab ScheduledGrab
        {
            get { return new ScheduledGrab()
                             {
                                 GrabMode = GrabTemplate.GrabMode,
                                 GrabParams = GrabTemplate.GrabParameters,
                                 GrabSchedule = GrabTemplate.GrabSchedule,
                                 Interval = 60
                             }; 
            }
        }

        public WinAddFromTemplate()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //lstGrabSchedule.ItemsSource = Enum.GetNames(typeof(Constants.GrabSchedules));
            //lstGrabMode.ItemsSource = Enum.GetNames(typeof(Constants.GrabModes));
            //try {
            //    string serializedGrabExpressions = Settings.Default.GrabExpressions;
            //    IEnumerable<string> grabExpressions = CsvGrabber.Core.Utils.DeserializeList(serializedGrabExpressions);
            //    if (grabExpressions.Any()) {
            //        foreach (string expression in grabExpressions) {
            //            cboExpressions.Items.Add(expression);
            //        }
            //        cboExpressions.SelectedIndex = 0;
            //    }
            //} catch (Exception ex) {
            //    Console.WriteLine("Error loading saved expression list: " + ex.Message);
            //    Settings.Default.GrabExpressions = CsvGrabber.Core.Utils.SerializeList(new string[0]);
            //}
        }

        /// <summary>
        /// Validates the settings selected.
        /// </summary>
        /// <returns><c>true</c> if the ettings selected are valid, otherwise <c>false</c></returns>
        private bool Validate()
        {
            bool valid = true;
            if (string.IsNullOrWhiteSpace(txtUserInput.Text))
            {
                DialogBox.ShowAlert(this, "Input is required", "Error");
                valid = false;
            }
            GrabTemplate.GrabParameters.Url = new GrabbableUrl(string.Format(GrabTemplate.UrlFormatString, txtUserInput.Text));
            return valid;
        }

        private void SaveGrab(object sender, ExecutedRoutedEventArgs e) {
            if (!Validate())
                return;
            DialogResult = true;
            Close();
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }
    }
}
