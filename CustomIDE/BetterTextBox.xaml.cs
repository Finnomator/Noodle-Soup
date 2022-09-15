using Colors;
using Newtonsoft.Json.Linq;
using Styles;
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
using System.Windows.Threading;

namespace CustomIDE {
    public partial class BetterTextBox : UserControl {

        Dictionary<Tuple<int, int>, LabelText> labelTextMap = new Dictionary<Tuple<int, int>, LabelText>();
        SuggestionBox suggestionBox = new SuggestionBox();
        string typingWord = "";
        readonly Dictionary<string, string> PatternMap = new Dictionary<string, string> {
            ["Words"] = @"[^\W\d](\w|[-']{1,2}(?=\w))*",
            ["Ints"] = @"\b\d+\b",
            ["Floats"] = @"\d+\.\d+",
            ["Special Chars"] = @"[^a-zA-Z\d\s]"
        };
        bool IgnoreWarning = false;

        public BetterTextBox() {
            InitializeComponent();
            suggestionBox.SuggestionClick += SuggestionButtonClick;
            MainGrid.Children.Add(suggestionBox);
        }

        public string GetText() {
            return MainTextBox.Text;
        }

        public void SetText(string text) {
            MainTextBox.Text = text;
        }
        void InsertText(string text, int index) {
            MainTextBox.Text = MainTextBox.Text.Insert(index, text);
        }

        void InsertAtCaret(string text) {
            InsertText(text, MainTextBox.CaretIndex);
            MainTextBox.CaretIndex += text.Length;
        }

        string GetWordAtIndex(int index) {
            string right = "";
            string left = "";
            string text = GetText();

            for (int i = index; i < text.Length && char.IsLetter(text[i]); i++) {
                try {
                    right += text[i];
                } catch (IndexOutOfRangeException) { break; }
            }

            for (int i = index - 1; i > -1 && char.IsLetter(text[i]); i--) {
                try {
                    left += text[i];

                } catch (IndexOutOfRangeException) { break; }
            }

            char[] charArray = left.ToCharArray();
            Array.Reverse(charArray);

            return new string(charArray) + right;
        }

        public IEnumerable<Tuple<string, int>> GetMatches(string text, string pattern) {
            MatchCollection matches = Regex.Matches(text, pattern);
            foreach (Match match in matches) {
                yield return Tuple.Create(match.Value, match.Index);
            }
        }

        int GetRowOfIndex(int index) {
            if (index < GetLine(0).Length)
                return 0;
            int i = -1;
            foreach (string line in GetLines(index)) {
                i++;
            }
            return i;
        }

        int GetColOfIndex(int index) {
            if (index == 0)
                return 0;
            string magenta = "";
            foreach (string line in GetLines(index)) {
                magenta += line;
            }
            int orange = magenta.Length - index;
            int red = GetLine(GetRowOfIndex(index)).Length - orange;
            return red;
        }

        IEnumerable<string> GetLines() {
            int lines = GetText().Split('\n').Length - 1;
            for (int i = 0; i < lines; i++) {
                yield return GetLine(i);
            }
        }

        IEnumerable<string> GetLines(int index) {
            int mover = 0;
            int i = 0;
            while (mover <= index) {
                string line = GetLine(i);

                yield return line;

                mover += line.Length;
                i++;
            }
        }

        string GetLine(int line) {
            return GetLine(line, GetText());
        }

        string GetLine(int line, string text) {
            string[] lines = text.Split('\n');
            if (line == lines.Length - 1)
                return lines[line];
            return lines[line] + '\n';
        }

        private void InsertSelectedSuggestionButton() {
            string toInsert = (string) suggestionBox.selectedButton.Content;
            toInsert = toInsert.Substring(typingWord.Length);
            InsertAtCaret(toInsert);
        }

        private void SuggestionButtonClick(object sender, EventArgs e) {
            Button clickedButton = (Button) sender;
            string toInsert = (string) clickedButton.Content;
            toInsert = toInsert.Substring(typingWord.Length);
            InsertAtCaret(toInsert);
        }


        private void TextChange(object sender, TextChangedEventArgs e) {
            TextGrid.Children.Clear();
            labelTextMap.Clear();
            string Text = GetText();
            string[] lines = Text.Split('\n');

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i].Replace("\r", "");
                string trimedLine = line.Trim();

                if (trimedLine.StartsWith("#")) {
                    TextGrid.Children.Add(new LabelText(line.TrimStart(), i, line.Length - line.TrimStart().Length, KeyWords.commentsColor));
                    continue;
                }

                foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Words"])) {
                    string text = word.Item1;
                    int index = word.Item2;
                    LabelText labelText = new LabelText(text, i, index, ColorDict.Get(text));
                    TextGrid.Children.Add(labelText);
                }

                foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Special Chars"])) {
                    string text = word.Item1;
                    int index = word.Item2;
                    LabelText labelText = new LabelText(text, i, index, ColorDict.Get(text));
                    TextGrid.Children.Add(labelText);
                }

                foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Ints"])) {
                    string text = word.Item1;
                    int index = word.Item2;
                    LabelText labelText = new LabelText(text, i, index, KeyWords.integerColor);
                    TextGrid.Children.Add(labelText);
                }

                foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Floats"])) {
                    string text = word.Item1;
                    int index = word.Item2;
                    LabelText labelText = new LabelText(text, i, index, KeyWords.FloatColor);
                    TextGrid.Children.Add(labelText);
                }
            }

            typingWord = GetWordAtIndex(MainTextBox.CaretIndex);

            if (GetLine(GetRowOfIndex(MainTextBox.CaretIndex - 1)).TrimStart().StartsWith("#"))
                typingWord = "";

            suggestionBox.Update(typingWord, GetColOfIndex(MainTextBox.CaretIndex - 1) * 8 + 8, GetRowOfIndex(MainTextBox.CaretIndex - 1) * 16 + 16);
        }


        private void TextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Up) {
                if (suggestionBox.isOpen && suggestionBox.selectedButton != null) {
                    suggestionBox.GoUp();
                    e.Handled = true;
                }
            } else if (e.Key == Key.Down) {
                if (suggestionBox.isOpen) {
                    suggestionBox.GoDown();
                    e.Handled = true;
                }
            } else if (e.Key == Key.Tab) {
                if (suggestionBox.isOpen) {
                    string toInsert;
                    if (suggestionBox.sugButtons.Count == 1) {
                        toInsert = (string) suggestionBox.sugButtons[0].Content;
                        toInsert = toInsert.Substring(typingWord.Length);
                        InsertAtCaret(toInsert);
                    } else if (suggestionBox.selectedButton != null) {
                        InsertSelectedSuggestionButton();
                    }
                } else {
                    InsertAtCaret("    ");
                }

                e.Handled = true;
            } else if (e.Key == Key.Return) {
                if (suggestionBox.isOpen && suggestionBox.selectedButton != null) {
                    InsertSelectedSuggestionButton();
                    e.Handled = true;
                }
            } else if (e.Key == Key.D8) {
                if (PressesShift()) {
                    InsertAtCaret("()");
                    e.Handled = true;
                } else if (Keyboard.IsKeyDown(Key.RightAlt)) {
                    InsertAtCaret("[]");
                    e.Handled = true;
                }
            } else if (e.Key == Key.D7 && Keyboard.IsKeyDown(Key.RightAlt)) {
                InsertAtCaret("{}");
                e.Handled = true;
            }
        }
        public static bool PressesShift() {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (IgnoreWarning || e.Text.Length != 1)
                return;

            int asciiByte = Encoding.ASCII.GetBytes(e.Text[0].ToString())[0];

            if (asciiByte < 256)
                return;

            MessageBoxResult messageBoxResult = MessageBox.Show($"The character ('{e.Text}') you typed may not be supported by the IDE/Font. Insert Anyway?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.No) {
                e.Handled = true;
            } else {
                IgnoreWarning = true;
            }
        }
    }



    public class LabelText : TextBlock {

        int Col;
        int Row;

        public LabelText(string content, int row, int col, Brush color) : base() {
            Foreground = color;
            Col = col;
            Row = row;
            FontFamily = new FontFamily("Cascadia Mono");
            Margin = new Thickness(Col * 8 + 3, Row * 16 + 1, 0, 0);
            Text = content;
            FontSize = 14;
            Panel.SetZIndex(this, 1);
            IsHitTestVisible = false;
            //TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            //TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
        }

        public void SetColor(Brush color) {
            Foreground = color;
        }
    }
}
