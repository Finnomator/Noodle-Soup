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
using System.Diagnostics;
using System.Threading;

namespace CustomIDE {
    public partial class MainWindow : Window {

        BrushConverter bc;
        string typing_word = "";
        bool last_was_colored = false;
        Dictionary<string, string> colors;
        Key[] word_end_chars = { Key.OemPeriod, Key.OemOpenBrackets, Key.OemCloseBrackets, Key.OemComma, Key.OemQuotes, Key.OemSemicolon, Key.Space, Key.Return };
        public MainWindow() {
            InitializeComponent();
            bc = new BrushConverter();
            colors = new Colors().colors;
        }

        void AppendText(RichTextBox box, string text, string color) {
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString(color));
        }

        private void OpenFileClick(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            TextRange tr = new TextRange(CodeBox.Document.ContentStart, CodeBox.Document.ContentEnd);
            if ((bool)!openFileDialog.ShowDialog())
                return;

            tr.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void OpenDirClick(object sender, RoutedEventArgs e) {

        }

        private void SaveClick(object sender, RoutedEventArgs e) {

        }

        void ColorWord() {
            if (typing_word == "")
                return;
            object color = bc.ConvertFromString(colors[colors.ContainsKey(typing_word) ? typing_word : "default"]);
            TextRange tr = new TextRange(CodeBox.CaretPosition.GetPositionAtOffset(-typing_word.Length), CodeBox.CaretPosition);
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        TextRange FindWordFromPos(TextPointer pos) {
            TextRange tr;
            
            tr = new TextRange(CodeBox.CaretPosition.GetPositionAtOffset(-1), pos);
            Debug.WriteLine(tr.Text);
            return tr;
        }

        private void KeyDown(object sender, KeyEventArgs e) {
            if (word_end_chars.Contains(e.Key)) {
                typing_word = "";
            } else if (e.Key == Key.Back) {
                if (typing_word != "")
                    typing_word = typing_word.Remove(typing_word.Length - 1);
                else {
                    TextRange tr = FindWordFromPos(CodeBox.CaretPosition);
                    typing_word = tr.Text;
                }
            } else if (e.Key.ToString().Length != 1) {
                return;
            } else {
                string letter = e.Key.ToString();

                if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))) {
                    letter = letter.ToLower();
                }
                typing_word += letter;
            }

            Debug.WriteLine("\"" + typing_word + "\"");
            ColorWord();
        }
    }
}
