using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core {
    public class MultipleExceptionLogger : ILogProvider {
        public List<Exception> MessageLog { get; set; }

        public void Log(string message) {
            return;
        }

        public void Log(string message, bool isError) {
            if(isError)
                MessageLog.Add(new Exception(message));
        }

        public void Log(string message, StatusMessage.StatusMessageType messageType) {
            if(messageType == StatusMessage.StatusMessageType.Error) {
                MessageLog.Add(new Exception(message));
            }
        }

        public void Log(StatusMessage message) {
            if(message.MessageType == StatusMessage.StatusMessageType.Error) {
                MessageLog.Add(new Exception(message.Message));
            }
        }

        public MultipleExceptionLogger() {
            MessageLog = new List<Exception>();
        }
    }
}
