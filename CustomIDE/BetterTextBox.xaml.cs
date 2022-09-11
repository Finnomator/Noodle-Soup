using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomIDE {
    /// <summary>
    /// Interaktionslogik für BetterTextBox.xaml
    /// </summary>
    public partial class BetterTextBox : UserControl {
        public BetterTextBox() {
            InitializeComponent();
        }

        public string GetText() {
            return MainTextBox.Text;
        }

        public void SetText(string text) {
            MainTextBox.Text = text;
        }

        public IEnumerable<Tuple<string, int>> GetAllWords() {
            string pattern = @"[^\W\d](\w|[-']{1,2}(?=\w))*";

            MatchCollection matches = Regex.Matches(GetText(), pattern);
            foreach (Match match in matches) {
                yield return Tuple.Create(match.Value, match.Index);
            }
        }

        int GetLengthOfTextUntilLine(int line) {
            int length = 0;
            string[] lines = GetText().Split('\n');
            for (int i = 0; i < lines.Length; i++) {
                string l = lines[i];
                length += l.Length;
                if (i == line)
                    return length;
            }
            throw new IndexOutOfRangeException();
        }

        int GetRowOfIndex(int index) {
            string[] lines = GetText().Split('\n');
            int lineIndex = 0;
            for (int i = 0; i < lines.Length; i++) {
                lineIndex += lines[i].Length;
                if (lineIndex >= index)
                    return i;
            }
            throw new IndexOutOfRangeException();
        }

        int GetColOfIndex(int index) {
            return index - GetLengthOfTextUntilLine(GetRowOfIndex(index));
        }

        private void TextChange(object sender, TextChangedEventArgs e) {
            MainGrid.Children.Clear();
            foreach (Tuple<string, int> word in GetAllWords()) {
                string text = word.Item1;
                int index = word.Item2;

                LabelText labelText = new LabelText(text, GetRowOfIndex(index), GetColOfIndex(index));
                MainGrid.Children.Add(labelText);
            }
        }
    }

    public class LabelText : TextBlock {

        int Col;
        int Row;

        public LabelText(string content, int row, int col) : base() {
            Foreground = Brushes.Red;
            Col = col;
            Row = row;
            FontFamily = new FontFamily("Cascadia Mono");
            Margin = new Thickness(Col*8 + 3, Row*16 + 1, 0, 0);
            Panel.SetZIndex(this, 2);
            Text = content;
            FontSize = 14;
        }

        public void SetColor(Brush color) {
            Foreground = color;
        }
    }
}
