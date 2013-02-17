using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core {
    public class ListLogger : ILogProvider {
        public List<string> MessageLog { get; set; }

        public void Log(string message) {
            return;
        }

        public void Log(string message, bool isError) {
            if(isError)
                MessageLog.Add(message);
        }

        public void Log(string message, StatusMessage.StatusMessageType messageType) {
            if(messageType == StatusMessage.StatusMessageType.Error) {
                MessageLog.Add(message);
            }
        }

        public void Log(StatusMessage message) {
            if(message.MessageType == StatusMessage.StatusMessageType.Error) {
                MessageLog.Add(message.Message);
            }
        }

        public ListLogger() {
            MessageLog = new List<string>();
        }
    }
}
