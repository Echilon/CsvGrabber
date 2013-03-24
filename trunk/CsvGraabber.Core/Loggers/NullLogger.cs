using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core {
    public class NullLogger : ILogProvider {
        public void Log(string message) {}
        public void Log(string message, bool isError) {}
        public void Log(string message, StatusMessage.StatusMessageType messageType) {}
        public void Log(StatusMessage message) {}
        public NullLogger() { }
    }
}
