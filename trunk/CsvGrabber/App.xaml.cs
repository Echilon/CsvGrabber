using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Lime49;
using NDesk.Options;

namespace CsvGrabber
{
    /// <summary>
    /// 
    /// </summary>
    public partial class App : Application
    {
        public App() {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WinUnhandledException dlg = new WinUnhandledException(e.Exception);
            dlg.ShowDialog();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            WinMain mainWindow;
            if (e.Args != null && e.Args.Length > 0) {
                string dbPath = null,
                       dbHost = null,
                       dbUser = null,
                       dbPass = null,
                       dbDatabase = null,
                       dbPrefix = string.Empty,
                       dbMode = null,
                       encryption = null,
                       password = null;
                int dbPort = -1;
                bool showHelp = false;
                OptionSet opts = new OptionSet() {
                    { "db:", "The database path", (string path) => dbPath = path },
                    { "dbport:", "The database port number", (int port) => dbPort = port },
                    { "dbuser:", "The database username", (user) => dbUser = user },
                    { "dbpass:", "The database password", (pass) => dbPass = pass },
                    { "h|?|help", v=> showHelp = true },
                };
                opts.Parse(e.Args);
                if (showHelp == true) {
                    Console.WriteLine("Check the help file or http://wiki.lime49.com for more information");
                    opts.WriteOptionDescriptions(Console.Out);
                    Shutdown();
                } else {

                }
            } else {
                mainWindow = new WinMain();
                mainWindow.ShowDialog();
            }
            Shutdown();
        }
    }
}
