﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CsvGrabber
{
    public class CsvGrabberCommands
    {
        public static RoutedUICommand Add = new RoutedUICommand("Add", "Add", typeof(CsvGrabberCommands));
        public static RoutedUICommand Browse = new RoutedUICommand("Browse", "Browse", typeof(CsvGrabberCommands));
        public static RoutedUICommand Delete = new RoutedUICommand("Delete", "Delete", typeof(CsvGrabberCommands));
        public static RoutedUICommand CopyUrl = new RoutedUICommand("CopyUrl", "CopyUrl", typeof(CsvGrabberCommands));
        public static RoutedUICommand Options = new RoutedUICommand("Options", "Options", typeof(CsvGrabberCommands));

        public static RoutedUICommand CopyForUrls = new RoutedUICommand("CopyForUrls", "CopyForUrls", typeof(CsvGrabberCommands));
        public static RoutedUICommand ManageTemplates = new RoutedUICommand("ManageTemplates", "ManageTemplates", typeof(CsvGrabberCommands));
    }
}
