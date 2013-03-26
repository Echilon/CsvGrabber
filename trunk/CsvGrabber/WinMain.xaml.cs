using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvGrabber.Core.Exporters;
using CsvGrabber.DAL;
using CsvGrabber.Properties;
using Lime49;
using Lime49.UI;
using Lime49.WPF;
using CsvGrabber.Core;
using System.ComponentModel;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Path = System.IO.Path;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace CsvGrabber
{
    /// <summary>
    /// Main window.
    /// </summary>
    public partial class WinMain : Window
    {
        private WPFConfiguration _config;
        private Grabber _grabber;
        /// <summary>
        /// Gets or sets the list of URLs to grab.
        /// </summary>
        /// <value>The list of URLs to grab.</value>
        public ObservableCollection<ScheduledGrab> ScheduledGrabs
        {
            get
            {
                ObjectDataProvider urlProvider = FindResource("urlsProvider") as ObjectDataProvider;
                return (urlProvider.ObjectInstance as CollectionBinder<ScheduledGrab>).Collection;
            }
            set
            {
                ObjectDataProvider urlProvider = FindResource("urlsProvider") as ObjectDataProvider;
                CollectionBinder<ScheduledGrab> binder = (urlProvider.ObjectInstance as CollectionBinder<ScheduledGrab>);
                if (binder == null) {
                    binder = new CollectionBinder<ScheduledGrab>();
                    urlProvider.ObjectInstance = binder;
                }
                binder.Collection = value;
            }
        }

        /// <summary>
        /// Gets the data provider for the list of templates.
        /// </summary>
        /// <value>The data provider for the list of templates.</value>
        private ObjectDataProvider TemplatesProvider {
            get { return FindResource("templatesProvider") as ObjectDataProvider; }
        }

        public WinMain() {
            InitializeComponent();
            ScheduledGrabs = new ObservableCollection<ScheduledGrab>();
        }

        /// <summary>
        /// Loads saved settings when the Window loads.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                //string serializedSavedUrls = Settings.Default.UrlList;
                //IEnumerable<GrabbableUrl> savedUrls = CsvGrabber.Core.Utils.DeserializeUrls(serializedSavedUrls);
                //ScheduledGrabs = new ObservableCollection<ScheduledGrab>(savedUrls);
                WPFConfiguration.LoadConfiguration();
                _config = WPFConfiguration.GetConfig();
                var db = new SqLiteDal();
                if (!db.EnsureExists()) {
                    DialogBox.ShowAlert(this, string.Format("The SQLite Database {0} could not be created", db.FileName), "Error");
                    Close();
                }
                RefreshScheduledGrabs();
            } catch (Exception ex) {
                Console.WriteLine("Error loading saved URL list: " + ex.Message);
            }

            try {
                string serializedFilenames = Settings.Default.FilePath;
                IEnumerable<string> items = CsvGrabber.Core.Utils.DeserializeList(serializedFilenames);
                if (items.Any()) {
                    foreach (string item in items) {
                        cboFileName.Items.Add(item);
                    }
                    cboFileName.SelectedIndex = 0;
                } else {
                    cboFileName.Text = System.IO.Path.Combine(Lime49.Utils.GetApplicationPath(), string.Format("grabbeddata{0:yyyy-MM-ddTHHmm}.csv", DateTime.Now));
                }
            } catch (Exception ex) {
                Console.WriteLine("Error loading saved expression list: " + ex.Message);
                Settings.Default.FilePath = CsvGrabber.Core.Utils.SerializeList(new string[0]);
            }

            cboDbPath.Text = Settings.Default.LogDbPath;
            /*string lastFilename = Settings.Default.FilePath;
            if (string.IsNullOrWhiteSpace(lastFilename)) {
                Settings.Default.FilePath = System.IO.Path.Combine(Lime49.Utils.GetApplicationPath(), string.Format("grabbeddata{0:yyyy-MM-ddTHHmm}.csv", DateTime.Now));
            }
            txtFilePath.Text = Settings.Default.FilePath;*/
            chkLogDatabase.IsChecked = Settings.Default.LogToDatabase;
            chkLogFile.IsChecked = Settings.Default.LogToFile;
            chkTrimWhitespace.IsChecked = Settings.Default.TrimExtraWhitespace;
            Settings.Default.Save();
        }

        private void CanRefreshSchedule(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsLoaded &&  (_grabber == null || (_grabber != null && !_grabber.IsBusy));
        }

        private void RefreshScheduledGrabs(object sender, ExecutedRoutedEventArgs e) {
            RefreshScheduledGrabs();
        }

        private void RefreshScheduledGrabs() {
            BackgroundWorker worker = new BackgroundWorker();
            ObservableCollection<ScheduledGrab> grabs = null;
            ObservableCollection<GrabTemplate> templates = null;
            worker.DoWork += (s, dwe) =>
                {
                    var db = new SqLiteDal();
                    grabs = new ObservableCollection<ScheduledGrab>(db.GetScheduledGrabs(null));
                    templates = new ObservableCollection<GrabTemplate>(db.GetTemplates());
                };
            worker.RunWorkerCompleted += (Fs, rwe) =>
                {
                    if (rwe.Error != null) {
                        DialogBox.ShowAlert(this, rwe.Error.Message, "Error");
                    } else {
                        ScheduledGrabs = grabs;
                        TemplatesProvider.ObjectInstance = templates;
                    }
                };
            worker.RunWorkerAsync();
            WPFUtils.WaitForWorker(worker);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Shows a file picker to choose a file.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void Browse(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.OriginalSource == cboFileName) {
                FolderBrowserDialog dlg = new FolderBrowserDialog()
                                              {
                                                  Description = "Browse",
                                                  SelectedPath = cboFileName.Text,

                                              };
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    if(e.OriginalSource == cboFileName) {
                        cboFileName.Text = dlg.SelectedPath;
                    }
                }
            } else if (e.OriginalSource == cboDbPath) {
                SaveFileDialog dlgSave = new SaveFileDialog()
                                             {
                                                 DefaultExt = ".csv",
                                                 Filter = "All Files|*.*|DB3 Files|*.db3",
                                                 FilterIndex = 2,
                                                 Title = "Browse",
                                                 FileName = cboDbPath.Text,
                                                 ValidateNames = false,
                                                 CheckPathExists = false
                                             };
                if(dlgSave.ShowDialog() == true) {
                        cboDbPath.Text = dlgSave.FileName;
                }
            }
        }

        #region CanExecute Handlers
        private void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Checks the grabber is not busy and not
        /// </summary>
        private void CanStartGrabbing(object sender, CanExecuteRoutedEventArgs e) {
            if(!IsLoaded) {
                e.CanExecute = false;
            } else {
                e.CanExecute =  _grabber == null || (_grabber != null && !_grabber.IsBusy);
            }
        }

        /// <summary>
        /// Checks the grabber is not busy.
        /// </summary>
        private void CanPauseGrabbing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoaded && _grabber != null && _grabber.IsBusy;
        }

        /// <summary>
        /// Checks the grabber is not busy.
        /// </summary>
        private void CanStopGrabbing(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoaded && _grabber != null && _grabber.IsBusy;
        }

        /// <summary>
        /// Checks whether a URL is selected in the list
        /// </summary>
        private void IsUrlSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoaded && lstUrls.SelectedItem != null;
        }

        private void CanAddUrl(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoaded && (_grabber == null || (_grabber != null && !_grabber.IsBusy));
        }

        private void CanDeleteUrl(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoaded && (_grabber == null || (_grabber != null && !_grabber.IsBusy)) && lstUrls.SelectedItem != null;
        }

        //private void CanClearAllUrls(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = IsLoaded && (_grabber == null || (_grabber != null && !_grabber.IsBusy)) && UrlsToGrab.Any();
        //}
        #endregion

        #region Misc Dialogs
        /// <summary>
        /// Shows the help file or a context sensitive tooltip.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void ShowHelp(object sender, ExecutedRoutedEventArgs e) {
            string helpType = Convert.ToString(e.Parameter);
            switch (helpType) {
                case "regex":
                    DialogBox.Show(this, "Extracts one value from each group in the regular expression", "Regular Expression Grab", DialogBoxType.OK, DialogBoxIcon.Question, DialogBoxButton.OK);
                    break;
                default:
                    WPFUtils.OpenHelp(null);
                    break;
            }
        }

        private void ShowOptions(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        ///// <summary>
        ///// Adds a new category when the return key is pressed.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        //private void txtUrl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.OemComma) {
        //        ApplicationCommands.New.Execute(this, null);
        //        e.Handled = true;
        //    }
        //}

        /// <summary>
        /// Adds a URL to the queue.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void AddUrl(object sender, ExecutedRoutedEventArgs e)
        {
            WinAddJob dlg = new WinAddJob()
            {
                Owner = this,
            };
            if(e.Parameter is GrabTemplate)
            {
                WinAddFromTemplate dlgTemplate = new WinAddFromTemplate(){ Owner=this};
                dlgTemplate.GrabTemplate = e.Parameter as GrabTemplate;
                if (dlg.ShowDialog() == true) {
                    dlg.CurrentGrab = dlgTemplate.ScheduledGrab;
                } else
                {
                    return;
                }
            }
            if (dlg.ShowDialog() == true) {
                BackgroundWorker worker = new BackgroundWorker();
                ScheduledGrab jobToGrab = dlg.CurrentGrab;
                worker.DoWork += (s, dwe) =>
                {
                    var db = new SqLiteDal();
                    db.AddScheduledGrabs(new[]{jobToGrab});
                };
                worker.RunWorkerCompleted += (Fs, rwe) =>
                {
                    if (rwe.Error != null) {
                        DialogBox.ShowAlert(this, rwe.Error.Message, "Error");
                    }
                };
                worker.RunWorkerAsync();
                WPFUtils.WaitForWorker(worker);
                CommandManager.InvalidateRequerySuggested();
                RefreshScheduledGrabs();
            }

            //string url = txtUrl.Text;
            //if (UrlsToGrab.Any(u=>u.Url.ToLowerInvariant() == url.ToLowerInvariant())) {
            //    DialogBox.ShowAlert(this, "URL already in queue", "Error");
            //} else if (!string.IsNullOrWhiteSpace(url)) {
            //    UrlsToGrab.Add(new GrabbableUrl(url));
            //    txtUrl.Text = string.Empty;
            //}
        }

        private void EditUrl(object sender, ExecutedRoutedEventArgs e)
        {
            ScheduledGrab url = lstUrls.SelectedValue as ScheduledGrab;
            WinAddJob dlg = new WinAddJob(url.Clone())
            {
                Owner = this,
            };
            if (dlg.ShowDialog() == true) {
                BackgroundWorker worker = new BackgroundWorker();
                ScheduledGrab jobToGrab = dlg.CurrentGrab;
                worker.DoWork += (s, dwe) =>
                {
                    var db = new SqLiteDal();
                    db.EditScheduledGrab(jobToGrab);
                };
                worker.RunWorkerCompleted += (Fs, rwe) =>
                {
                    if (rwe.Error != null) {
                        DialogBox.ShowAlert(this, rwe.Error.Message, "Error");
                    }
                };
                worker.RunWorkerAsync();
                WPFUtils.WaitForWorker(worker);
                CommandManager.InvalidateRequerySuggested();
                RefreshScheduledGrabs();
            }
        }

        /// <summary>
        /// Deletes a URL from the Queue.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void DeleteUrl(object sender, ExecutedRoutedEventArgs e) {
            if(DialogBox.Show(this, "Really delete this URL", "Confirm Deletion", DialogBoxType.YesNo, DialogBoxIcon.Question, DialogBoxButton.No) != DialogBoxButton.Yes)
                return;
            ScheduledGrab url = lstUrls.SelectedValue as ScheduledGrab;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, dwe) => {
                                 var db = new SqLiteDal();
                                 db.DeleteScheduledGrab(url);
                             };
            worker.RunWorkerCompleted += (Fs, rwe) => {
                                             if(rwe.Error != null) {
                                                 DialogBox.ShowAlert(this, rwe.Error.Message, "Error");
                                             }
                                         };
            worker.RunWorkerAsync();
            WPFUtils.WaitForWorker(worker);
            CommandManager.InvalidateRequerySuggested();
            RefreshScheduledGrabs();
        }

        /// <summary>
        /// Clears all URLs from the Queue.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        //private void ClearAllUrls(object sender, ExecutedRoutedEventArgs e) {
        //    UrlsToGrab = new ObservableCollection<GrabbableUrl>();
        //    Settings.Default.UrlList = CsvGrabber.Core.Utils.SerializeUrls(UrlsToGrab);
        //    Settings.Default.Save();
        //}

        private void CopyUrl(object sender, ExecutedRoutedEventArgs e) {
            GrabbableUrl url = e.Parameter as GrabbableUrl;
            if (url == null)
                return;
            Clipboard.SetText(url.Url);
        }

        private void CopyForUrls(object sender, ExecutedRoutedEventArgs e) {
            ScheduledGrab url = e.Parameter as ScheduledGrab;
            if (url == null)
                return;
            var dlg = new WinUserInput()
                          {
                              Prompt = "Enter URLs",
                              AcceptsReturn = true,
                              Owner = this
                          };
            if(dlg.ShowDialog()==true)
            {
                var rawUrls = dlg.UserInput;
                if(string.IsNullOrWhiteSpace(rawUrls))
                    return;
                BackgroundWorker saveWorker = new BackgroundWorker();
            saveWorker.DoWork += (s, dwe) => {
                                     
                var db = new SqLiteDal();
                                                 var urls = rawUrls.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                                                 db.DuplicateGrabs(url, urls);
            };
            saveWorker.RunWorkerCompleted += (Fs, rwe) => {
                                                 if(rwe.Error != null) {
                                                     DialogBox.ShowAlert(this, rwe.Error.Message, "Error Saving Job Results");
                                                 }
                NavigationCommands.Refresh.Execute(null, this);
                                             };
            saveWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Validates, then starts grabbing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void StartGrabbing(object sender, ExecutedRoutedEventArgs e)
        {
            var scheduledGrab = e.Parameter as ScheduledGrab;
            List<string> paths = new List<string>();
            foreach (var item in cboFileName.Items) {
                string path = Convert.ToString(item);
                if (!string.IsNullOrWhiteSpace(path) && !paths.Contains(path)) {
                    paths.Add(path);
                }
            }
            paths.RemoveAll(p => p == cboFileName.Text);
            if (!string.IsNullOrWhiteSpace(cboFileName.Text)) {
                paths.Insert(0, cboFileName.Text);
                cboFileName.Items.Insert(0, cboFileName.Text);
            };
            Settings.Default.LogToDatabase = chkLogDatabase.IsChecked == true;
            Settings.Default.LogToFile = chkLogFile.IsChecked == true;
            Settings.Default.TrimExtraWhitespace = chkTrimWhitespace.IsChecked == true;
            Settings.Default.LogDbPath = cboDbPath.Text;
            Settings.Default.FilePath = CsvGrabber.Core.Utils.SerializeList(paths);

            Settings.Default.Save();
            _config = WPFConfiguration.GetConfig();
            _grabber = new Grabber();
            _grabber.Logger = new CompositeLogger(new ConsoleLogger(), new RichTextBoxLogger(rtbLog));
            _grabber.GrabComplete += new EventHandler<GrabCompleteEventArgs>(grabber_GrabComplete);
            _grabber.GrabFailed += new EventHandler(grabber_GrabFailed);
            if (scheduledGrab == null)
            {

                _grabber.Start();
            } else {
                _grabber.GrabSingle(scheduledGrab);
            }
        }

        private void PauseGrabbing(object sender, ExecutedRoutedEventArgs e) {
            _grabber.TogglePause();
        }

        private void StopGrabbing(object sender, ExecutedRoutedEventArgs e)
        {
            _grabber.Abort();
        }

        private void grabber_GrabFailed(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Saves the results of a grab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CsvGrabber.Core.GrabCompleteEventArgs"/> instance containing the event data.</param>
        private void grabber_GrabComplete(object sender, GrabCompleteEventArgs e) {
            BackgroundWorker saveWorker = new BackgroundWorker();
            saveWorker.DoWork += (s, dwe) => {
                                     if(_config.LogToDatabase) {
                                         var db = new SqLiteDal();
                                         db.AddJobHistory(new[] {e.Job});
                                     }
                                     if(_config.LogToFile) {
                                         string filePath= CsvGrabber.Core.Utils.DeserializeList(_config.FilePath).FirstOrDefault();
                                         DirectoryInfo jobLogDir = new DirectoryInfo(System.IO.Path.Combine(filePath, Core.Utils.SanitizeFileName(e.Job.ScheduledGrab.Name)));
                                         if (!jobLogDir.Exists)
                                             Directory.CreateDirectory(jobLogDir.FullName); //jobLogDir.Create();
                                         switch (e.Job.ScheduledGrab.GrabMode) {
                                             case Constants.GrabModes.Regex:
                                                 string logPath = System.IO.Path.Combine(jobLogDir.FullName, string.Format("{0:yyyy-MM-ddTHHmm}.csv", DateTime.Now));
                                                 CSVExporter exporter = new CSVExporter(logPath, _config.AppendLogFile) {IncludeRawResponse = _config.LogRawResponse, TrimExtraWhitespace=_config.TrimExtraWhitespace};
                                                 exporter.Save(e.Job.Response);
                                                 break;
                                                 case Constants.GrabModes.Scrape:
                                                 int read = 0;
                                                 byte[] buffer = new byte[1024];
                                                 string fileName = System.IO.Path.GetFileNameWithoutExtension(e.Job.ScheduledGrab.GrabParams.Url.Url);
                                                 string extension = System.IO.Path.GetExtension(e.Job.ScheduledGrab.GrabParams.Url.Url);
                                                 string outputFile = System.IO.Path.Combine(jobLogDir.FullName, string.Format("{0}-{1:yyyy-MM-ddTHHmm}{2}", fileName, DateTime.Now, extension));
                                                 using (MemoryStream ms = new MemoryStream(e.Job.Response.BinaryResponse))
                                                 using(FileStream fs = new FileStream (outputFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)){//= new BinaryWriter()) {
                                                     while ((read = ms.Read(buffer, 0, buffer.Length)) > 0) {
                                                         ms.Write(buffer, 0, read);
                                                     }
                                                 }
                                                 break;
                                         }
                                     }
                                 };
            saveWorker.RunWorkerCompleted += (Fs, rwe) => {
                                                 if(rwe.Error != null) {
                                                     Dispatcher.BeginInvoke(new ThreadStart(() =>
                                                         { DialogBox.ShowAlert(this, rwe.Error.Message, "Error Saving Job Results"); }));
                                                 }
                                             };
            saveWorker.RunWorkerAsync();
            CommandManager.InvalidateRequerySuggested();
        }

        private void lstUrls_MouseDoubleClick(object sender, MouseButtonEventArgs e){
            ApplicationCommands.Properties.Execute(null, this);
        }

        private void ManageTemplates(object sender, ExecutedRoutedEventArgs e)
        {
            WinManageTemplates dlg = new WinManageTemplates() {Owner = this};
            dlg.ShowDialog();
            NavigationCommands.Refresh.Execute(null, this);
        }
    }
}