﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using CsvGrabber.Core;
using CsvGrabber.Core.Exporters;
using CsvGrabber.DAL;

namespace CsvGrabber
{
    /// <summary>
    /// Extracts matches from a list of URL using a regular expression.
    /// </summary>
    public class Grabber:DependencyObject
    {
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(Grabber), new PropertyMetadata(false));
        public bool IsBusy {
            get { return (bool)base.GetValue(IsBusyProperty); }
            set { base.SetValue(IsBusyProperty, value); }
        }

        public event EventHandler GrabStarted;
        public event EventHandler<GrabCompleteEventArgs> GrabComplete;
        public event EventHandler GrabFailed;

        public ObservableCollection<GrabJob> Jobs { get; set; }
        public GrabEventArgs Arguments { get; set; }
        public bool IsPaused { get; private set; }
        private WPFConfiguration _config;
        //public int ThreadCount { get; set; }

        public ILogProvider Logger; 
        private bool _cancellationPending = false;
        private ManualResetEvent[] _handles = new ManualResetEvent[0];

        public Grabber() {
            Jobs = new ObservableCollection<GrabJob>();
            Logger = new ConsoleLogger();
            Arguments = new GrabEventArgs();
            //ThreadCount = 4;
            _config = WPFConfiguration.GetConfig();
        }

        public Grabber(GrabEventArgs args)
            : this() {
            this.Arguments = args;
        }

        /// <summary>
        /// Starts threads for grabbing Urls using the specified arguments.
        /// </summary>
        public void Start() {
            _cancellationPending = false;
            IsBusy = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, dwe) =>
            {
                var db = new SqLiteDal();
                var jobs = db.GetScheduledGrabs(true);
                dwe.Result = jobs;
                //ManualResetEvent[] waitEvents = new ManualResetEvent[ThreadCount];
                //for (int i = 0; i < ThreadCount; i++) {
                //    try {
                //        ManualResetEvent doneEvent = new ManualResetEvent(false);
                //        if (ThreadPool.QueueUserWorkItem(new WaitCallback(ParseUrl), new UrlGrabThreadInfo(doneEvent, i))) {
                //            waitEvents[i] = doneEvent;
                //            _logger.Log(string.Format("Started download thread #{0}", i));
                //        } else {
                //            _logger.Log(string.Format("Error starting download thread #{0}", i), true);
                //        }
                //    } catch (Exception ex) {
                //        _logger.Log(string.Format("Error starting download thread #{0}", i, true) + ": " + ex.Message, true);
                //    }
                //}
                //WaitHandle.WaitAll(waitEvents);
                //dwe.Result = new GrabCompleteEventArgs(ParsedUrls);
            };
            worker.RunWorkerCompleted += (s, rwe) =>
            {
                if (rwe.Error != null) {
                    Logger.Log("Error starting grabbing URLs" + ": " + rwe.Error.Message, true);
                    if (GrabFailed != null)
                        GrabFailed(this, EventArgs.Empty);
                    IsBusy = false;
                } else {
                    Jobs = new ObservableCollection<GrabJob>();
                    var jobsToRun = rwe.Result as IEnumerable<ScheduledGrab>;
                    GrabUrls(jobsToRun);
                }
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Grabs a single url.
        /// </summary>
        /// <param name="scheduledGrab">The scheduled grab.</param>
        public void GrabSingle(ScheduledGrab scheduledGrab) {
            GrabUrls(new[]{scheduledGrab});
        }

        /// <summary>
        /// Grabs a batch of urls.
        /// </summary>
        /// <param name="scheduledGrabs">The scheduled grabs.</param>
        public void GrabUrls(IEnumerable<ScheduledGrab> grabs) {
            int jobCount = grabs.Count();
            _handles = new ManualResetEvent[jobCount];
            for (int i = 0; i < jobCount; i++) {
                var job = grabs.ElementAt(i);
                _handles[i] = new ManualResetEvent(false);
                if (_cancellationPending)
                    return;
                GrabJob jobToRun = new GrabJob(job);
                jobToRun.Logger = this.Logger;
                jobToRun.WaitIndex = i;
                jobToRun.GrabComplete += new EventHandler<GrabCompleteEventArgs>(jobToRun_GrabComplete);
                jobToRun.GrabFailed += GrabFailed;
                jobToRun.GrabStarted += GrabStarted;
                Jobs.Add(jobToRun);
                jobToRun.Start();
                //GrabCompleteEventArgs args = rwe.Result as GrabCompleteEventArgs;
                //if (args != null) {
                //    if(GrabComplete != null)
                //        GrabComplete(this, args);
                //    _logger.Log("Grabbing complete");
                //}
                //WaitHandle.WaitAll(_handles);
                //
            }
        }

        private void jobToRun_GrabComplete(object sender, GrabCompleteEventArgs e)
        {
            BackgroundWorker saveWorker = new BackgroundWorker();
            saveWorker.DoWork += (s, dwe) =>
            {
                if (_config.LogToDatabase) {
                    var db = new SqLiteDal(_config.DatabasePath);
                    db.EnsureExists();
                        db.AddJobHistory(new[] { e.Job });
                }
                if (_config.LogToFile) {
                    string filePath= CsvGrabber.Core.Utils.DeserializeList(_config.FilePath).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(filePath))
                        filePath = Lime49.Utils.GetApplicationPath();
                    string fileName = Path.GetExtension(filePath);
                    var logDirectory = string.IsNullOrWhiteSpace(fileName)? filePath: Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(logDirectory))
                        Directory.CreateDirectory(logDirectory);
                    switch (e.Job.ScheduledGrab.GrabMode) {
                        case Constants.GrabModes.Regex:
                            string logFilePath = System.IO.Path.Combine(logDirectory, string.Format("{0}-{1:yyyy-MM-ddTHHmm}.csv", Utils.SanitizeFileName(e.Job.ScheduledGrab.Name), DateTime.Now));
                            CSVExporter exporter = new CSVExporter(logFilePath, _config.AppendLogFile) { IncludeRawResponse = _config.LogRawResponse, TrimExtraWhitespace = _config.TrimExtraWhitespace };
                            exporter.Save(e.Job.Response);
                            break;
                        case Constants.GrabModes.Scrape:
                            //todo: test-untested
                            string dumpPath = System.IO.Path.Combine(logDirectory, string.Format("{0}-{1:yyyy-MM-ddTHHmm}.csv", Utils.SanitizeFileName(e.Job.ScheduledGrab.Name), DateTime.Now));
                            File.WriteAllBytes(dumpPath, e.Job.Response.BinaryResponse);
                            break;
                    }
                }
            };
            saveWorker.RunWorkerCompleted += (Fs, rwe) =>
            {
                if (rwe.Error != null) {
                    Logger.Log(string.Format("Failed saving output for job #{0} - {1}: {2}", e.Job.ScheduledGrab.GrabID, e.Job.ScheduledGrab.Name, rwe.Error.Message));
                    GrabFailed(sender, e);
                }
                _handles[e.Job.WaitIndex].Set();
                if (e.Job.WaitIndex == _handles.Length - 1)
                {
                    Dispatcher.Invoke(new Action(() =>
                                                     {
                                                         IsBusy = false;
                                                     }));
                }
            };
            saveWorker.RunWorkerAsync();
            if (e.Job.ScheduledGrab.GrabSchedule == Constants.GrabSchedules.OneTime)
                Jobs.Remove(e.Job);
            if (GrabComplete != null)
                GrabComplete(sender, e);
        }

        /// <summary>
        /// Pauses or resumes grabbing without aborting.
        /// </summary>
        public void TogglePause()
        {
            IsPaused = !IsPaused;
            foreach (var job in Jobs) {
                job.TogglePause();
            }
            Logger.Log("Grabbing paused");
        }

        /// <summary>
        /// Aborts grabbing and terminates thread.
        /// </summary>
        public void Abort()
        {
            _cancellationPending = true;
            foreach (var job in Jobs) {
                job.Stop();
            }
            Logger.Log("Aborted grabbing");
        }
    }
}
