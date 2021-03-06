﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Collections;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Drawing;
using CsvGrabber.Core;
using System.Windows;
using System.Windows.Input;
using Utils = Lime49.Utils;

namespace CsvGrabber.Converters {
    /// <summary>
    /// Performs the opposite of BoolToVisibilityConverter, converting True to Visibility.Collapsed and False to Visibility.Hidden
    /// </summary>
    public class BoolToInvertedVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                bool b = (bool)value;
                return b ? Visibility.Collapsed : Visibility.Visible;
            } catch {
                return 1;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public sealed class NullToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string param = System.Convert.ToString(parameter);
            if (param == "invert") {
                return value == null ? Visibility.Visible : Visibility.Hidden;
            } else {
                return value == null ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns <c>false</c> if the string equivalent of <c>value</c> and <c>parameter</c> are equal, otherwise <c>true</c>.
    /// </summary>
    public class StringEqualityToInvertedBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                string val = System.Convert.ToString(value);
                string otherVal = System.Convert.ToString(parameter);
                return val != otherVal;
            } catch {
                return true;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts a boolean value.
    /// </summary>
    public class BooleanInverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                bool b = (bool)value;
                return !b;
            } catch {
                return true;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Converts a byte array to an image source.
    /// </summary>
    public class ByteToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || !(value is byte[])) {
                return value;
            }
            BitmapImage img = new BitmapImage();
            using(MemoryStream ms = new MemoryStream((byte[])value)) {
                img.BeginInit();
                img.StreamSource = ms;
                img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
            }
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Checks a value is not null, returning true if it <b>is not</b> null, and false if it is null.
    /// </summary>
    public class TrueIfNullOrEmptyConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value == null || (value as string ?? string.Empty).Trim().Length == 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Checks a value is not null, returning true if it <b>is not</b> null, and false if it is null.
    /// </summary>
    public class TrueIfNotNullConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value != null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a CultureInfo to a string representing a flag image for that culture.
    /// </summary>
    public class CultureInfoToFlagConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || !(value is CultureInfo)) {
                return null;
            } else {
                return string.Format("pack://application:,,,/Resources/images/flags/{0}.png", Utils.GetISO3166(value as CultureInfo));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class RegexBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is Regex) {
                return ((Regex)value).ToString();
            } else {
                return value;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string rgxString = System.Convert.ToString(value);
            try
            {
                return string.IsNullOrWhiteSpace(rgxString) ? null : new Regex(rgxString);
            }
            catch {
                return null;
            }
        }
    }

    public class GrabScheduleEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is Constants.GrabSchedules) {
                return ((Constants.GrabSchedules)value).ToString();
            } else {
                return value;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Constants.GrabSchedules.OneTime : (Constants.GrabSchedules)Enum.Parse(typeof(Constants.GrabSchedules), System.Convert.ToString(value), true);
        }
    }

    public class GrabModeEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is Constants.GrabModes) {
                return ((Constants.GrabModes)value).ToString();
            } else {
                return value;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return string.IsNullOrWhiteSpace(value as string) ? Constants.GrabModes.Scrape : (Constants.GrabModes)Enum.Parse(typeof(Constants.GrabModes), System.Convert.ToString(value), true);
        }
    }

    public class GrabSourceEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value is Constants.GrabSource) {
                return ((Constants.GrabSource)value).ToString();
            } else {
                return value;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return string.IsNullOrWhiteSpace(value as string) ? Constants.GrabSource.Url : (Constants.GrabSource)Enum.Parse(typeof(Constants.GrabSource), System.Convert.ToString(value), true);
        }
    }

    public class ScheduledJobArmedStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool isArmed = System.Convert.ToBoolean(value);
            return isArmed ?
                "pack://application:,,,/Resources/images/grab_active_16.png" :
                "pack://application:,,,/Resources/images/grab_inactive_16.png";

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception();
        }
    }

    public class TemplateMenuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<GrabTemplate> templates = value as IEnumerable<GrabTemplate>;
            if (templates == null)
                return value;
            List<Control> itms = new List<Control>();
            foreach(var template in templates)
            {
                itms.Add(new MenuItem()
                             {
                                 Command = ApplicationCommands.New,
                                 CommandParameter = template
                             });
            }
            itms.Add(new Separator());
            itms.Add(new MenuItem()
                         {
                             Command = CsvGrabberCommands.ManageTemplates,
                             Header = "Manage Templates"
                         });
            return itms;

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new Exception();
        }
    }

    /// <summary>
    /// Provides a breakpoint for debugging
    /// </summary>
    public class DebugConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    #region Converters for the WPF toolkit Datagrid
    public class BoolToOpacityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            try {
                bool b = (bool)value;
                if(b) return 1; else return 0;
            } catch {
                return 1;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class ObjectToTypeName : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if(value != null) {
                return value.GetType().Name;
            } else {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    #endregion
}
