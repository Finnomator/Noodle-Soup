using Colors;
using Styles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NoodleSoup {
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
        private int TextBoxRowHeight = 16;
        private int TextBoxColumnWidth = 8;

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

            string text = MainTextBox.Text;

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

                if (Ctrl) {
                    switch (other_key) {
                        case Key.Space:
                            suggestionBox.Update(typingWord,
                                                 GetColOfIndex(MainTextBox.CaretIndex - 1, text) * TextBoxColumnWidth + TextBoxColumnWidth,
                                                 GetRowOfIndex(MainTextBox.CaretIndex - 1, text) * TextBoxRowHeight + TextBoxRowHeight,
                                                 true);
                            break;
                        case Key.A:
                            MainTextBox.SelectAll();

                            break;
                        case Key.Back:
                            CutOutString(MainTextBox.CaretIndex - typingWord.Length, MainTextBox.CaretIndex);
                            typingWord = "";
                            break;
                        case Key.C:
                            if (MainTextBox.SelectionLength > 0)
                                Clipboard.SetText(MainTextBox.SelectedText);
                            break;
                        case Key.V:
                            if (MainTextBox.SelectionLength > 0)
                                CutOutString(MainTextBox.SelectionStart, MainTextBox.SelectionStart + MainTextBox.SelectionLength);
                            InsertAtCaret(Clipboard.GetText());
                            break;
                        case Key.X:
                            string selected_text = MainTextBox.SelectedText;
                            if (selected_text != "") {
                                Clipboard.SetText(selected_text);
                                CutOutString(MainTextBox.SelectionStart, MainTextBox.SelectionStart + MainTextBox.SelectionLength);
                            }
                            break;
                        case Key.Left:
                            int old_idx = MainTextBox.CaretIndex;
                            int w_len = GetWordLeftOfIndex(old_idx).Length;
                            MainTextBox.CaretIndex -= w_len < 1 && old_idx != 0 ? 1 : w_len;

                            if (old_idx == MainTextBox.CaretIndex && MainTextBox.CaretIndex - 2 > -1)
                                MainTextBox.CaretIndex -= 2;

                            break;
                        case Key.Right:
                            int wr_len = GetWordRightOfIndex(MainTextBox.CaretIndex).Length;
                            MainTextBox.CaretIndex += wr_len < 1 ? 1 : wr_len;
                            break;
                    }
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
            MainTextBox.Text = text.Substring(0, start) + text.Substring(end);
            MainTextBox.CaretIndex = start;
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

        private string GetWordLeftOfIndex(int index) {

            if (index == 0)
                return "";

            string left = "";
            string text = GetText();

            for (int i = index - 1; i > -1 && char.IsLetter(text[i]); i--) {
                try {
                    left += text[i];

                } catch (IndexOutOfRangeException) { break; }
            }

            char[] charArray = left.ToCharArray();
            Array.Reverse(charArray);

            return new string(charArray);
        }

        private string GetWordRightOfIndex(int index) {
            string right = "";
            string text = GetText();

            for (int i = index; i < text.Length && char.IsLetter(text[i]); i++) {
                try {
                    right += text[i];
                } catch (IndexOutOfRangeException) { break; }
            }

            return right;
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

        int GetRowOfIndex(int index, string text) {
            if (index < GetLine(0, text).Length)
                return 0;
            int i = -1;
            foreach (string line in GetLines(index, text)) {
                i++;
            }
            return i;
        }

        int GetColOfIndex(int index, string text) {
            if (index == 0)
                return 0;
            string magenta = "";
            foreach (string line in GetLines(index, text)) {
                magenta += line;
            }
            int orange = magenta.Length - index;
            int red = GetLine(GetRowOfIndex(index, text), text).Length - orange;
            return red;
        }

        IEnumerable<string> GetLines(int index, string text) {
            int mover = 0;
            int i = 0;
            while (mover <= index) {
                string line = GetLine(i, text);
                yield return line;
                mover += line.Length;
                i++;
            }
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

        private int GetScrollViewerRowOffset() {
            MainScrollViewer.UpdateLayout();
            return (int) (MainScrollViewer.ContentVerticalOffset / TextBoxRowHeight);
        }

        public int[] GetDisplayedRows(string text) {

            int start_row = GetScrollViewerRowOffset();

            int visible_lines = 0;

            int scroll_viewer_rows = (int) (MainScrollViewer.ActualHeight / TextBoxRowHeight);

            while (visible_lines <= scroll_viewer_rows) {
                try {
                    GetLine(start_row + visible_lines, text);
                    visible_lines++;
                } catch (IndexOutOfRangeException) {
                    break;
                }
            }

            return new int[] { start_row, start_row + visible_lines };
        }

        private string GetDisplayedText(string text) {
            string res = "";
            int[] row_start_end = GetDisplayedRows(text);
            for (int i = row_start_end[0]; i < row_start_end[1]; i++) {
                res += GetLine(i, text);
            }
            return res;
        }

        private void Indexing() {
            while (true) {

                while (!BeginnIndexing)
                    Thread.Sleep(10);

                Application.Current.Dispatcher.Invoke(new Action(() => {

                    TextGrid.Children.Clear();
                    string text = GetText();
                    string displayed_text = GetDisplayedText(text);
                    string[] lines = displayed_text.Split('\n');
                    int scroll_viewer_row_offset = GetScrollViewerRowOffset();

                    for (int i = 0; i < lines.Length; i++) {
                        string line = lines[i].Replace("\r", "");
                        string trimedLine = line.Trim();

                        if (trimedLine.Length == 0)
                            continue;

                        int row = i + scroll_viewer_row_offset;

                        if (trimedLine.StartsWith("#")) {
                            TextGrid.Children.Add(new LabelText(line.TrimStart(), row, line.Length - line.TrimStart().Length, KeyWords.commentsColor));
                            continue;
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Words"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, row, word.Item2, ColorDict.Get(word.Item1)));
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Special Chars"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, row, word.Item2, ColorDict.Get(word.Item1)));
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Ints"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, row, word.Item2, ColorDict.Get(word.Item1)));
                        }

                        foreach (Tuple<string, int> word in GetMatches(line, PatternMap["Floats"])) {
                            TextGrid.Children.Add(new LabelText(word.Item1, row, word.Item2, ColorDict.Get(word.Item1)));
                        }
                    }

                    typingWord = GetWordAtIndex(MainTextBox.CaretIndex);

                    if (GetLine(GetRowOfIndex(MainTextBox.CaretIndex - 1, text), text).TrimStart().StartsWith("#"))
                        typingWord = "";

                    suggestionBox.Update(typingWord, GetColOfIndex(MainTextBox.CaretIndex - 1, text) * TextBoxColumnWidth + TextBoxColumnWidth, GetRowOfIndex(MainTextBox.CaretIndex - 1, text) * TextBoxRowHeight + TextBoxRowHeight);

                    BeginnIndexing = false;

                }));
            }
        }

        private void TextChange(object sender, TextChangedEventArgs e) {
            BeginnIndexing = true;

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

        private void MainScrollViwer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            BeginnIndexing = true;
        }
    }


    public class LabelText : TextBlock {

        private int Col;
        private int Row;

        public LabelText(string content, int row, int col, Brush color, int textBoxRowHeight = 16, int textBoxColWidth = 8) : base() {
            Foreground = color;
            Col = col;
            Row = row;
            FontFamily = new FontFamily("Cascadia Mono");
            Margin = new Thickness(Col * textBoxColWidth + 3, Row * textBoxRowHeight + 1, 0, 0);
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
