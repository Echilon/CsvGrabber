using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CsvGrabber.Core;
using CsvGrabber.Properties;
using Lime49.UI;

namespace CsvGrabber
{
    public partial class WinUserInput: Window
    {
        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(WinUserInput), new PropertyMetadata(true));
        public bool AcceptsReturn {
            get { return (bool) base.GetValue(AcceptsReturnProperty); }
            set { base.SetValue(AcceptsReturnProperty, value); }
        }

        public static readonly DependencyProperty UserInputProperty = DependencyProperty.Register("UserInput", typeof(string), typeof(WinUserInput), new PropertyMetadata(string.Empty));
        public string UserInput {
            get { return base.GetValue(UserInputProperty) as string; }
            set { base.SetValue(UserInputProperty, value); }
        }

        public static readonly DependencyProperty PromptProperty = DependencyProperty.Register("Prompt", typeof(string), typeof(WinUserInput), new PropertyMetadata(string.Empty));
        public string Prompt {
            get { return base.GetValue(PromptProperty) as string; }
            set { base.SetValue(PromptProperty, value); }
        }

        public WinUserInput()
        {
            InitializeComponent();
        }

        private void SaveGrab(object sender, ExecutedRoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }
    }
}
