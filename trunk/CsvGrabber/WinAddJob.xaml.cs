﻿using System;
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
    /// Interaction logic for WinAddJob.xaml
    /// </summary>
    public partial class WinAddJob : Window
    {
        public static readonly DependencyProperty CurrentGrabProperty = DependencyProperty.Register("CurrentGrab", typeof(ScheduledGrab), typeof(WinAddJob), new PropertyMetadata(new ScheduledGrab(),Grab_Changed));

        private static void Grab_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (WinAddJob) d;
            var newGrab = e.NewValue as ScheduledGrab;
            if(newGrab==null||newGrab.GrabParams==null||newGrab.GrabParams.GrabExpression==null)
                return;
            ctl.chkFreeSpacing.IsChecked = (newGrab.GrabParams.GrabExpression.Options.HasFlag(RegexOptions.IgnorePatternWhitespace));
            ctl.chkDotNewLine.IsChecked = (newGrab.GrabParams.GrabExpression.Options.HasFlag(RegexOptions.Singleline));
            ctl.chkCaseInsensitive.IsChecked = (newGrab.GrabParams.GrabExpression.Options.HasFlag(RegexOptions.IgnoreCase));
            ctl.cboExpressions.GetBindingExpression(ComboBox.TextProperty).UpdateTarget();
        }

        /// <summary>
        /// Gets or sets the grab being added/edited.
        /// </summary>
        /// <value>The grab.</value>
        public ScheduledGrab CurrentGrab {
            get { return base.GetValue(CurrentGrabProperty) as ScheduledGrab; }
            set { base.SetValue(CurrentGrabProperty, value); }
        }

        public WinAddJob()
        {
            InitializeComponent();
            CurrentGrab = new ScheduledGrab();
        }

        public WinAddJob(ScheduledGrab url)
        :this(){
            this.CurrentGrab = url;
            this.Title = "Edit Job";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            lstGrabSchedule.ItemsSource = Enum.GetNames(typeof(Constants.GrabSchedules));
            lstGrabMode.ItemsSource = Enum.GetNames(typeof(Constants.GrabModes));
            try {
                string serializedGrabExpressions = Settings.Default.GrabExpressions;
                IEnumerable<string> grabExpressions = CsvGrabber.Core.Utils.DeserializeList(serializedGrabExpressions);
                if (grabExpressions.Any()) {
                    foreach (string expression in grabExpressions) {
                        cboExpressions.Items.Insert(0,expression);
                    }
                    if (CurrentGrab != null && CurrentGrab.GrabParams.GrabExpression !=null&& !string.IsNullOrWhiteSpace(CurrentGrab.GrabParams.GrabExpression.ToString()))
                    {
                        var regexString = CurrentGrab.GrabParams.GrabExpression.ToString();
                        if (!cboExpressions.Items.Contains(regexString))
                            cboExpressions.Items.Insert(0,regexString);
                        cboExpressions.SelectedItem = regexString;
                    } else
                    {
                        cboExpressions.SelectedIndex = 0;
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("Error loading saved expression list: " + ex.Message);
                Settings.Default.GrabExpressions = CsvGrabber.Core.Utils.SerializeList(new string[0]);
            }
        }

        /// <summary>
        /// Displays the right set of options depending on the grab type selected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void lstGrabMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            Constants.GrabModes selectedGrabMode = (Constants.GrabModes)Enum.Parse(typeof(Constants.GrabModes), Convert.ToString(lstGrabMode.SelectedValue ?? Constants.GrabModes.Regex.ToString()), true);
            switch (selectedGrabMode) {
                case Constants.GrabModes.Regex:
                    tbcWizard.SelectedIndex = tbcWizard.Items.IndexOf(tabGrabRegex);
                    break;
            }
        }

        private void lstGrabSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            Constants.GrabSchedules selectedGrabSchedule = (Constants.GrabSchedules)Enum.Parse(typeof(Constants.GrabSchedules), Convert.ToString(lstGrabSchedule.SelectedItem ?? Constants.GrabSchedules.OneTime.ToString()), true);
            switch (selectedGrabSchedule) {
                case Constants.GrabSchedules.OneTime:
                    tbcWizardSchedule.SelectedIndex = tbcWizardSchedule.Items.IndexOf(tabGrabScheduleOneTime);
                    break;
                case Constants.GrabSchedules.Interval:
                    tbcWizardSchedule.SelectedIndex = tbcWizardSchedule.Items.IndexOf(tabGrabScheduleInterval);
                    break;
            }
        }

        /// <summary>
        /// Validates the settings selected.
        /// </summary>
        /// <returns><c>true</c> if the ettings selected are valid, otherwise <c>false</c></returns>
        private bool Validate()
        {
            bool valid = true;
            Constants.GrabModes selectedGrabMode = (Constants.GrabModes)Enum.Parse(typeof(Constants.GrabModes), Convert.ToString(lstGrabMode.SelectedItem ?? Constants.GrabModes.Regex.ToString()), true);
            switch (selectedGrabMode) {
                case Constants.GrabModes.Regex:
                    if (string.IsNullOrWhiteSpace(cboExpressions.Text)) {
                        DialogBox.ShowAlert(this, "A regular expression to match is required", "Error");
                        valid = false;
                    } else {
                        try {
                            new Regex(cboExpressions.Text);
                        } catch {
                            DialogBox.ShowAlert(this, string.Format("Invalid regular expression: {0}", cboExpressions.Text), "Error");
                            valid = false;
                        }
                    }
                    break;
            }

            Constants.GrabSchedules selectedGrabSchedule = (Constants.GrabSchedules)Enum.Parse(typeof(Constants.GrabSchedules), Convert.ToString(lstGrabSchedule.SelectedItem ?? Constants.GrabSchedules.OneTime.ToString()), true);
            switch (selectedGrabSchedule) {
                case Constants.GrabSchedules.OneTime:
                    // nothing to validate
                    break;
                case Constants.GrabSchedules.Interval:
                    int interval;
                    if (!int.TryParse(txtGrabInterval.Text, out interval) || interval <=0) {
                        DialogBox.ShowAlert(this, "The minimum interval is once per second", "Error");
                        valid = false;
                    }
                    break;
            }
            return valid;
        }

        private void SaveGrab(object sender, ExecutedRoutedEventArgs e) {
            if (!Validate())
                return;
            //Settings.Default.UrlList = CsvGrabber.Core.Utils.SerializeUrls(UrlsToGrab);
            List<string> expressions = new List<string>();
            foreach (var exp in cboExpressions.Items) {
                string expression = Convert.ToString(exp);
                if (!string.IsNullOrWhiteSpace(expression) && !expressions.Contains(expression)) {
                    expressions.Insert(0, expression);
                }
            }
            if (!expressions.Contains(cboExpressions.Text) && !string.IsNullOrWhiteSpace(cboExpressions.Text)) {
                expressions.Insert(0,cboExpressions.Text);
                cboExpressions.Items.Insert(0, cboExpressions.Text);
            }
            Settings.Default.GrabExpressions = CsvGrabber.Core.Utils.SerializeList(expressions);

            Settings.Default.Save();
            GrabEventArgs args = new GrabEventArgs(new GrabbableUrl(txtUrl.Text));
            RegexOptions options = 0;
            if (chkCaseInsensitive.IsChecked == true) {
                options |= RegexOptions.IgnoreCase;
            }
            if (chkDotNewLine.IsChecked == true) {
                options |= RegexOptions.Singleline;
            }
            if (chkFreeSpacing.IsChecked == true) {
                options |= RegexOptions.IgnorePatternWhitespace;
            }
            args.GrabExpression = new Regex(cboExpressions.Text, options);

            CurrentGrab.GrabParams = args;
            DialogResult = true;
            Close();
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }
    }
}