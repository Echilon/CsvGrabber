using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using CsvGrabber.DAL;
using CsvGrabber.Properties;
using Lime49.UI;

namespace CsvGrabber
{
    /// <summary>
    /// Interaction logic for WinAddJob.xaml
    /// </summary>
    public partial class WinManageTemplates: Window
    {
        public static readonly DependencyProperty TemplatesProperty = DependencyProperty.Register("Templates", typeof(ObservableCollection<GrabTemplate>), typeof(WinManageTemplates), new PropertyMetadata(new ObservableCollection<GrabTemplate>()));
        public ObservableCollection<GrabTemplate> Templates {
            get { return base.GetValue(TemplatesProperty) as ObservableCollection<GrabTemplate>; }
            set { base.SetValue(TemplatesProperty, value); }
        }

         /// <summary>
        /// Gets or sets the template being added/edited.
        /// </summary>
        /// <value>The template.</value>
        public GrabTemplate CurrentTemplate
        {
            get { return (lstTemplates.SelectedItem as GrabTemplate); }
            set { lstTemplates.SelectedItem = value; }
        }

        public WinManageTemplates()
        {
            InitializeComponent();
            CurrentTemplate = new GrabTemplate();
        }

        public WinManageTemplates(GrabTemplate template)
        :this(){
            this.CurrentTemplate = template;
            this.Title = "Edit Template";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            lstGrabSchedule.ItemsSource = Enum.GetNames(typeof(Constants.GrabSchedules));
            lstGrabMode.ItemsSource = Enum.GetNames(typeof(Constants.GrabModes));
            try {
                string serializedGrabExpressions = Settings.Default.GrabExpressions;
                IEnumerable<string> grabExpressions = CsvGrabber.Core.Utils.DeserializeList(serializedGrabExpressions);
                if (grabExpressions.Any()) {
                    foreach (string expression in grabExpressions) {
                        cboExpressions.Items.Add(expression);
                    }
                    cboExpressions.SelectedIndex = 0;
                }
            } catch (Exception ex) {
                Console.WriteLine("Error loading saved expression list: " + ex.Message);
                Settings.Default.GrabExpressions = CsvGrabber.Core.Utils.SerializeList(new string[0]);
            }
            NavigationCommands.Refresh.Execute(null, this);
        }

        private void RefreshTemplates(object sender, RoutedEventArgs e)
        {
            List<GrabTemplate> templates = new List<GrabTemplate>();
            BackgroundWorker saveWorker = new BackgroundWorker();
            saveWorker.DoWork += (s, dwe) =>
                                     {
                                         var db = new SqLiteDal();
                                         templates = db.GetTemplates();
                                     };
            saveWorker.RunWorkerCompleted += (Fs, rwe) =>
                                                 {
                                                     if (rwe.Error != null)
                                                     {
                                                         DialogBox.ShowAlert(this, rwe.Error.Message, "Error Saving Job Results");
                                                     } else
                                                     {
                                                         this.Templates = new ObservableCollection<GrabTemplate>(templates);
                                                     }
                                                 };
            saveWorker.RunWorkerAsync();
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

            if (string.IsNullOrWhiteSpace(txtUrlFormatString.Text))
            {
                DialogBox.ShowAlert(this, "URL format string is required", "Error");
                valid = false;
                txtUrlFormatString.Focus();
            }
            return valid;
        }

        private void SaveTemplate(object sender, ExecutedRoutedEventArgs e) {
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
                expressions.Add(cboExpressions.Text);
                cboExpressions.Items.Add(cboExpressions.Text);
            }
            Settings.Default.GrabExpressions = CsvGrabber.Core.Utils.SerializeList(expressions);

            Settings.Default.Save();
            GrabEventArgs args = new GrabEventArgs(new GrabbableUrl(txtUrlFormatString.Text));
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

            CurrentTemplate.GrabParameters = args;

            var templateToSave = CurrentTemplate;
            BackgroundWorker saveWorker = new BackgroundWorker();
            saveWorker.DoWork += (s, dwe) =>
            {
                    var db = new SqLiteDal();
                    if (string.IsNullOrWhiteSpace(templateToSave.TemplateID)) {
                        templateToSave.TemplateID = Lime49.Utils.GenID(true);
                        db.AddGrabTemplates(new[] { templateToSave });
                    } else
                    {
                        db.EditGrabTemplate(templateToSave);
                    }
            };
            saveWorker.RunWorkerCompleted += (Fs, rwe) =>
            {
                if (rwe.Error != null) {
                    DialogBox.ShowAlert(this, rwe.Error.Message, "Error Saving Job Results");
                } else
                {
                    CurrentTemplate = templateToSave;
                }
            };
            saveWorker.RunWorkerAsync();
        }

        private void DeleteTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            if (DialogBox.Show(this, "Really delete this template", "Confirm Deletion", DialogBoxType.YesNo, DialogBoxIcon.Question, DialogBoxButton.No) != DialogBoxButton.Yes)
                return;
            var template = CurrentTemplate;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, dwe) =>
                                 {
                                     var db = new SqLiteDal();
                                     db.DeleteGrabTemplate(template);
                                 };
            worker.RunWorkerCompleted += (Fs, rwe) =>
                                             {
                                                 if (rwe.Error != null)
                                                 {
                                                     DialogBox.ShowAlert(this, rwe.Error.Message, "Error");
                                                 }
                                             };
            worker.RunWorkerAsync();
            WPFUtils.WaitForWorker(worker);
            CommandManager.InvalidateRequerySuggested();
            NavigationCommands.Refresh.Execute(null,this);
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

        private void CreateTemplate(object sender, ExecutedRoutedEventArgs e)
        {
            var tmp = new GrabTemplate()
                          {
                              Name = "Untitled",
                              TemplateID = string.Empty
                          };
            Templates.Add(tmp);
            lstTemplates.SelectedItem = tmp;
        }

        private void IsTemplateSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoaded && lstTemplates.SelectedItem is GrabTemplate;
        }
    }
}
