using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core
{
    public class ScheduledGrab : EventArgs, INotifyPropertyChanged
    {
        private int _grabId;
        private int _interval;
        private string _name;
        private bool _isActive;
        private Constants.GrabSchedules _grabSchedule;
        private Constants.GrabModes _grabMode;
        private Constants.GrabSource _grabSource;
        private GrabEventArgs _grabParams;

        public int GrabID {
            get { return _grabId; }
            set
            {
                _grabId = value;
                OnPropertyChanged("GrabID");
            }
        }

        public int Interval {
            get { return _interval; }
            set
            {
                _interval = value;
                OnPropertyChanged("Interval");
            }
        }

        public string Name {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public bool IsActive {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        public Constants.GrabSchedules GrabSchedule {
            get { return _grabSchedule; }
            set
            {
                _grabSchedule = value;
                OnPropertyChanged("GrabSchedule");
            }
        }

        public Constants.GrabSource GrabSource
        {
            get { return _grabSource; }
            set
            {
                _grabSource = value;
                OnPropertyChanged("GrabSource");
            }
        }

        public Constants.GrabModes GrabMode
        {
            get { return _grabMode; }
            set
            {
                _grabMode = value;
                OnPropertyChanged("GrabMode");
            }
        }

        /// <summary>
        /// Gets or sets the parameters used to grab, eg: a regex.
        /// </summary>
        /// <value>The parameters used to grab, eg: a regex.</value>
        public GrabEventArgs GrabParams {
            get { return _grabParams; }
            set
            {
                _grabParams = value;
                OnPropertyChanged("GrabParams");
            }
        }

        public ScheduledGrab() {
            IsActive = true;
            Interval = 1000;
            GrabParams = new GrabEventArgs();
            GrabSchedule = Constants.GrabSchedules.OneTime;
            GrabMode = Constants.GrabModes.Scrape;
            GrabSource = Constants.GrabSource.Url;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string strPropertyName)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(strPropertyName));
            }
        }

        public ScheduledGrab Clone()
        {
            var clone= new ScheduledGrab()
                       {
                           GrabID = this.GrabID,
                           GrabMode = this.GrabMode,
                           GrabParams = this.GrabParams,
                           GrabSchedule = this.GrabSchedule,
                           Interval = this.Interval,
                           IsActive = this.IsActive,
                           Name = this.Name,
                           GrabSource=this.GrabSource
                       };
            return clone;
        }
    }
}
