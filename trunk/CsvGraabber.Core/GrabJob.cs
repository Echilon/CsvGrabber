using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Timer = System.Timers.Timer;

namespace CsvGrabber.Core
{
    /// <summary>
    /// Extracts matches from a list of URL using a regular expression.
    /// </summary>
    public class GrabJob
    {
        public event EventHandler GrabStarted;
        public event EventHandler<GrabCompleteEventArgs> GrabComplete;
        public event EventHandler GrabFailed;

        public ScheduledGrab ScheduledGrab { get; set; }
        public bool IsBusy { get; private set; }

        private System.Timers.Timer tmr;
        public ILogProvider Logger { get; set; }

        public int WaitIndex { get; set;}

        public GrabJob() {
            Logger = new ConsoleLogger();
            tmr = new Timer();
            tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);
        }

        public GrabJob(ScheduledGrab grab)
            : this() {
            this.ScheduledGrab = grab;
        }

        private void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(ScheduledGrab.GrabSchedule==Constants.GrabSchedules.OneTime)
                tmr.Stop();
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, dwe) =>
            {
                if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                    Thread.CurrentThread.Name = string.Format("Grab Job #{0} - Thread #{1}" , ScheduledGrab.GrabID, Thread.CurrentThread.ManagedThreadId);
                Logger.Log(string.Format("Grabbing Job #{0} on thread {1}" , ScheduledGrab.GrabID, Thread.CurrentThread.ManagedThreadId));
                GrabbedJob grabbedJob = new GrabbedJob(this, null);
                
                //bool finished = false;
                //while (!finished) {
                    GrabEventArgs grapParams = ScheduledGrab.GrabParams;
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + grabbedJob.WaitIndex);
                            if(!string.IsNullOrWhiteSpace(grapParams.Url.Url)) {
                                try {
                                    GrabResponse response = new GrabResponse();
                                    switch (ScheduledGrab.GrabMode) {
                                        case Constants.GrabModes.Regex:
                                            response.RawResponse = GetStringContents(ScheduledGrab.GrabSource, grapParams.Url.Url);
                                            int numExtracted = 0;
                                            MatchCollection matches = grapParams.GrabExpression.Matches(response.RawResponse);
                                            foreach(Match match in matches) {
                                                if(match.Success) {
                                                    List<string> captures = new List<string>();
                                                    for(int i=1;i<match.Groups.Count;i++) {
                                                        Group matchGroup=match.Groups[i];
                                                        foreach(Capture capture in matchGroup.Captures) {
                                                            captures.Add(capture.Value);
                                                        }
                                                    }
                                                    response.ParsedResponse.Add(captures);
                                                    numExtracted++;
                                                }
                                            }
                                            grabbedJob.Response = response;
                                            grabbedJob.Result = Constants.GrabResult.Success;
                                            Logger.Log(string.Format("Extracted {0} matches from {1}", numExtracted, grapParams.Url.Url));
                                            break;
                                        case Constants.GrabModes.Scrape:
                                            response.BinaryResponse = GetBinaryContents(ScheduledGrab.GrabSource, grapParams.Url.Url);
                                            Logger.Log(string.Format("Extracted page content from {0} - {1} bytes", grapParams.Url.Url, response.BinaryResponse.Length));
                                            break;
                                    }
                                } catch(Exception ex) {
                                    grabbedJob.Result = Constants.GrabResult.Fail;
                                    Logger.Log(string.Format("Error reading matches from {0}", grapParams.Url.Url) + ": " + ex.Message, true);
                                }
                            }
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId+":"+grabbedJob.WaitIndex);
                                 dwe.Result = new GrabCompleteEventArgs(grabbedJob);
                                 //}
            };
            worker.RunWorkerCompleted += (s, rwe) =>
            {
                IsBusy = false;
                if (rwe.Error != null) {
                    Logger.Log("Error grabbing URLs" + ": " + rwe.Error.Message, true);
                    if (GrabFailed != null)
                        GrabFailed(this, EventArgs.Empty);
                } else {
                    GrabCompleteEventArgs args = rwe.Result as GrabCompleteEventArgs;
                    if (args != null) {
                        if (GrabComplete != null)
                            GrabComplete(this, args);
                        Logger.Log(string.Format("Grabbing of job #{0} ({1}) complete", ScheduledGrab.GrabID, ScheduledGrab.Name));
                    }
                }
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Gets the binary contents.
        /// </summary>
        /// <param name="grabSource">The grab source.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static byte[] GetBinaryContents(Constants.GrabSource grabSource, string url) {
            var contents = new byte[0];
            switch (grabSource) {
                case Constants.GrabSource.Url:
                    byte[] buffer = new byte[1024];
                    int read;
                    using (WebClient client = new WebClient())
                    using (Stream responseStream = client.OpenRead(url))
                    using (BinaryReader reader = new BinaryReader(responseStream, Encoding.UTF8))
                    using (MemoryStream ms = new MemoryStream()) {
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0) {
                            ms.Write(buffer, 0, read);
                        }
                        contents = ms.ToArray();
                    }
                    break;
                case Constants.GrabSource.File:
                    contents = File.ReadAllBytes(url);
                    break;
            }
            return contents;
        }

        private static string GetStringContents(Constants.GrabSource grabSource, string url)
        {
            string contents = null;
            switch (grabSource) {
                case Constants.GrabSource.Url:
                    using (WebClient client = new WebClient())
                    using (Stream responseStream = client.OpenRead(url))
                    using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8)) {
                        contents = reader.ReadToEnd();
                    }
                    break;
                case Constants.GrabSource.File:
                    contents = File.ReadAllText(url);
                    break;
            }
            return contents;
        }

        /// <summary>
        /// Starts threads for grabbing Urls using the specified arguments.
        /// </summary>
        public void Start() {
            tmr.Interval = ScheduledGrab.Interval;
            tmr.Start();
        }

        public void TogglePause()
        {
            if (tmr != null) {
                if (tmr.Enabled) {
                    tmr.Stop();
                } else {
                    tmr.Start();
                }
            }
        }

        public void Stop() {
            if (tmr != null)
                tmr.Stop();
        }
    }
}
