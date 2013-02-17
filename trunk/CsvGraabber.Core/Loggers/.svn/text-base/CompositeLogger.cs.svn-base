using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsvGrabber.Core {
    /// <summary>
    /// Logs messages to multiple log providers
    /// </summary>
    public class CompositeLogger : ILogProvider {
        private List<ILogProvider> loggers;

        /// <summary>
        /// Initializes a new <see cref="CompositeLogger"/>.
        /// </summary>
        public CompositeLogger() {
            loggers = new List<ILogProvider>();
        }

        /// <summary>
        /// Initializes a new <see cref="CompositeLogger"/> for the specified loggers.
        /// </summary>
        /// <param name="loggers">The providers to which to log.</param>
        public CompositeLogger(params ILogProvider[] loggers) 
            :this() {
            this.loggers.AddRange(loggers);
        }

        public void AddProvider(ILogProvider provider) {
            loggers.Add(provider);
        }

        /// <summary>
        /// Removes a log provider if it exists.
        /// </summary>
        /// <param name="provider">The provider to remove.</param>
        public void RemoveProvider(ILogProvider provider) {
            loggers.Remove(provider);
        }

        /// <summary>
        /// Removes all providers of a specified type.
        /// </summary>
        /// <param name="providerType">Type of the provider.</param>
        public void RemoveAllProviders(Type providerType) {
            loggers.RemoveAll(p => p.GetType() == providerType);
        }

        /// <summary>
        /// Gets all providers of a specified type.
        /// </summary>
        /// <typeparam name="TResult">Type of the provider.</typeparam>
        /// <returns>A collection of all providers of a specified type</returns>
        public IEnumerable<TResult> GetProviders<TResult>() {
            return this.loggers.OfType<TResult>();
        }

        public void Log(string message) {
            foreach(ILogProvider logger in loggers) {
                logger.Log(message);
            }
        }

        public void Log(string message, bool isError) {
            foreach(ILogProvider logger in loggers) {
                logger.Log(message, isError);
            }
        }

        public void Log(string message, StatusMessage.StatusMessageType messageType) {
            Log(new StatusMessage(message, messageType));
        }

        public void Log(StatusMessage message) {
            foreach(ILogProvider logger in loggers) {
                logger.Log(message);
            }
        }
    }
}
