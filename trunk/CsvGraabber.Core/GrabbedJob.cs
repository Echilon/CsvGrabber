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
    ///
    /// </summary>
    public class GrabbedJob:GrabJob
    {
        public int ItemID { get; set; }
        public DateTime GrabDate { get; set; }
        public Constants.GrabResult Result { get; set; }
        public GrabResponse Response { get; set; }

        public GrabbedJob():base() {
            GrabDate = DateTime.Now;
        }

        public GrabbedJob(ScheduledGrab grab, GrabResponse response)
            :base(grab) {
                GrabDate = DateTime.Now;
                this.Response = response;
        }

        public GrabbedJob(GrabJob baseJob, GrabResponse response)
            : this() {
            this.ScheduledGrab = baseJob.ScheduledGrab;
            this.Response = response;
            this.WaitIndex = baseJob.WaitIndex;
            }
    }
}
