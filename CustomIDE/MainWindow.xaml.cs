using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace CustomIDE {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            AppendText(CodeBox, "Test", "Red");
        }

        void AppendText(RichTextBox box, string text, string color) {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            } catch (FormatException) { }
        }

        private void OpenFileClick(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //if (openFileDialog.ShowDialog() == true)
                //CodeBox. = File.ReadAllText(openFileDialog.FileName);
        }

        private void OpenDirClick(object sender, RoutedEventArgs e) {
            
        }

        private void SaveClick(object sender, RoutedEventArgs e) {

        }

    }
}
