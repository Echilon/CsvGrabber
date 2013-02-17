using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core {
    public class ConsoleLogger : ILogProvider {

        public void Log(string message) {
            Console.WriteLine(message);
        }

        public void Log(string message, bool isError) {
            Console.WriteLine(message);
        }

        public void Log(string message, StatusMessage.StatusMessageType messageType) {
            Log(new StatusMessage(message, messageType));
        }

        public void Log(StatusMessage message) {
            Console.Write(message.Message + (message.Multipart || message.Message.EndsWith("\n") ? "" : "\n"));
        }

        public ConsoleLogger() {}
    }
}
