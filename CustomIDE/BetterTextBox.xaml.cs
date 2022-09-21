using Colors;
using NoodleSoup;
using Styles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        public delegate void HotkeyHandler(object sender, HotkeyEventArgs e);
        public event HotkeyHandler HotkeyPressed;

        private SuggestionBox suggestionBox = new SuggestionBox();
        private string typingWord = "";
        private Dictionary<string, string> PatternMap = new Dictionary<string, string> {
            ["Words"] = @"[^\W\d](\w|[-']{1,2}(?=\w))*",
            ["Ints"] = @"\b\d+\b",
            ["Floats"] = @"\d+\.\d+",
            ["Special Chars"] = @"[^a-zA-Z\d\s]"
        };
        private bool IgnoreWarning = false;
        private bool BeginnIndexing = false;

        public BetterTextBox() {
            InitializeComponent();
            suggestionBox.SuggestionClick += SuggestionButtonClick;
            Grid.SetColumn(suggestionBox, 1);
            MainGrid.Children.Add(suggestionBox);
            LinesBox.Tag = 1;
            HotkeyPressed += BetterTextBox_HotkeyPressed;
            Thread Indexer = new Thread(new ThreadStart(Indexing));
            Indexer.Start();
            Indexer.IsBackground = true;
        }

        private void BetterTextBox_HotkeyPressed(object sender, HotkeyEventArgs e) {
            if (e.PressedKeys.Length == 2) {
                bool Ctrl = false;
                Key other_key = Key.None;

                if (e.PressedKeys.Contains(Key.LeftCtrl)) {
                    Ctrl = true;
                    other_key = e.PressedKeys[e.PressedKeys[0] == Key.LeftCtrl ? 1 : 0];
                } else if (e.PressedKeys.Contains(Key.RightCtrl)) {
                    Ctrl = true;
                    other_key = e.PressedKeys[e.PressedKeys[0] == Key.RightCtrl ? 1 : 0];
                }


                if (Ctrl && other_key == Key.Space)
                    suggestionBox.Update(typingWord, GetColOfIndex(MainTextBox.CaretIndex - 1) * 8 + 8, GetRowOfIndex(MainTextBox.CaretIndex - 1) * 16 + 16, true);
                else if (Ctrl && other_key == Key.A)
                    MainTextBox.SelectAll();
                else if (Ctrl && other_key == Key.Back) {
                    CutOutString(MainTextBox.CaretIndex - typingWord.Length, MainTextBox.CaretIndex);
                    typingWord = "";
                } else if (Ctrl && other_key == Key.C) {
                    if (MainTextBox.SelectionLength > 0)
                        Clipboard.SetText(MainTextBox.SelectedText);
                } else if (Ctrl && other_key == Key.X) {
                    string selected_text = MainTextBox.SelectedText;
                    if (selected_text != "") {
                        Clipboard.SetText(selected_text);
                        CutOutString(MainTextBox.SelectionStart, MainTextBox.SelectionStart + MainTextBox.SelectionLength);
                    }
                } else if (Ctrl && other_key == Key.V) {
                    if (MainTextBox.SelectionLength > 0)
                        CutOutString(MainTextBox.SelectionStart, MainTextBox.SelectionStart + MainTextBox.SelectionLength);
                    InsertAtCaret(Clipboard.GetText());
                }
            }
        }

        public string GetText() {
            return MainTextBox.Text;
        }

        public void SetText(string text) {
            MainTextBox.Text = text;
        }

        public void CutOutString(int start, int end) {
            string text = MainTextBox.Text;
            int oldidx = MainTextBox.CaretIndex;
            MainTextBox.Text = text.Substring(0, start) + text.Substring(end);
            MainTextBox.CaretIndex = oldidx;
        }

        public void InsertText(string text, int index) {
            int old_caret_pos = MainTextBox.CaretIndex;
            MainTextBox.Text = MainTextBox.Text.Insert(index, text);
            MainTextBox.CaretIndex = old_caret_pos;
        }

        public void InsertAtCaret(string text) {
            InsertAtCaret(text, text.Length);
        }

        public void InsertAtCaret(string text, int caret_offset) {
            InsertText(text, MainTextBox.CaretIndex);
            MainTextBox.CaretIndex += caret_offset;
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

        private void Indexing() {
            while (true) {

                while (!BeginnIndexing)
                    Thread.Sleep(10);

                Application.Current.Dispatcher.Invoke(new Action(() => {

                    TextGrid.Children.Clear();
                    string Text = GetText();
                    string[] lines = Text.Split('\n');

                    int linesBoxDelta = lines.Length - (int) LinesBox.Tag;
                    LinesBox.Tag = lines.Length;

                    if (linesBoxDelta != 0) {
                        int last_length = lines.Length.ToString().Length;
                        LinesBox.Text = new string('0', last_length - 1) + "1\n";
                        for (int i = 2; i < lines.Length + 1; i++) {
                            LinesBox.Text += new string('0', last_length - i.ToString().Length) + i + "\n";
                        }
                    }

                    for (int i = 0; i < lines.Length; i++) {
                        string line = lines[i].Replace("\r", "");
                        string trimedLine = line.Trim();

                        if (trimedLine.StartsWith("#")) {
                            TextGrid.Children.Add(new LabelText(line.TrimStart(), i, line.Length - line.TrimStart().Length, KeyWords.commentsColor));
                            continue;
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Words"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, i, word.Item2, ColorDict.Get(word.Item1)));
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Special Chars"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, i, word.Item2, ColorDict.Get(word.Item1)));
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Ints"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, i, word.Item2, ColorDict.Get(word.Item1)));
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Floats"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, i, word.Item2, ColorDict.Get(word.Item1)));
                        }
                    }

                    typingWord = GetWordAtIndex(MainTextBox.CaretIndex);

                    if (GetLine(GetRowOfIndex(MainTextBox.CaretIndex - 1)).TrimStart().StartsWith("#"))
                        typingWord = "";

                    suggestionBox.Update(typingWord, GetColOfIndex(MainTextBox.CaretIndex - 1) * 8 + 8, GetRowOfIndex(MainTextBox.CaretIndex - 1) * 16 + 16);

                    BeginnIndexing = false;
                }));
            }
        }

        private void TextChange(object sender, TextChangedEventArgs e) {
            BeginnIndexing = true;
        }


        private void TextBox_KeyDown(object sender, KeyEventArgs e) {

            switch (e.Key) {
                case Key.Up:
                    if (suggestionBox.isOpen && suggestionBox.selectedButton != null) {
                        suggestionBox.GoUp();
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (suggestionBox.isOpen) {
                        suggestionBox.GoDown();
                        e.Handled = true;
                    }
                    break;
                case Key.Tab:
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
                    break;
                case Key.Return:
                    if (suggestionBox.isOpen && suggestionBox.selectedButton != null) {
                        InsertSelectedSuggestionButton();
                        e.Handled = true;
                    }
                    break;
                case Key.D8:
                    if (PressesShift()) {
                        InsertAtCaret("()", 1);
                        e.Handled = true;
                    } else if (Keyboard.IsKeyDown(Key.RightAlt)) {
                        InsertAtCaret("[]", 1);
                        e.Handled = true;
                    }
                    break;
                case Key.D7:
                    if (Keyboard.IsKeyDown(Key.RightAlt)) {
                        InsertAtCaret("{}", 1);
                        e.Handled = true;
                    }
                    break;

                default:
                    if (PressesStrg()) {
                        HotkeyPressed(this, new HotkeyEventArgs(GetAllPressedKeys()));
                        e.Handled = true;
                    }
                    break;
            }
        }

        public static Key[] GetAllPressedKeys() {
            var keys = new List<Key>();
            foreach (Key x in Enum.GetValues(typeof(Key)).Cast<Key>()) {
                try {
                    if (Keyboard.IsKeyDown(x))
                        keys.Add(x);
                } catch (System.ComponentModel.InvalidEnumArgumentException) {
                    continue;
                }
            }
            return keys.ToArray();
        }

        public static bool PressesShift() {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }

        public static bool PressesStrg() {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        private void Box_PreviewTextInput(object sender, TextCompositionEventArgs e) {
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

        private void MainTextBox_LostFocus(object sender, RoutedEventArgs e) {
            suggestionBox.Close();
        }

    }


    public class LabelText : TextBlock {

        int Col;
        int Row;

        public LabelText(string content, int row, int col, Brush color) : base() {
            Foreground = color;
            Col = col;
            Row = row;
            FontFamily = new FontFamily("Cascadia Code");
            Margin = new Thickness(Col * 8 + 3, Row * 16 + 1, 0, 0);
            Text = content;
            FontSize = 14;
            IsHitTestVisible = false;
        }

        public void SetColor(Brush color) {
            Foreground = color;
        }
    }

    public class HotkeyEventArgs : EventArgs {
        public Key[] PressedKeys { get; private set; }
        public HotkeyEventArgs(Key[] keys) {
            PressedKeys = keys;
        }
    }

    public class IndexingProgressEventArgs : EventArgs {
        public int Progress { get; private set; }
        public IndexingProgressEventArgs(int progress) {
            Progress = progress;
        }
    }
}
