using Colors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Styles;

namespace CustomIDE {
    public partial class CoderBox : UserControl {
        int lines_count = 1;
        Highlighter highlighter;
        Thread marker;
        bool highlight = false;
        string typingWord = "";
        SuggestionBox suggestionBox;
        Matcher matcher;

        public CoderBox() {
            InitializeComponent();
            matcher = new Matcher(KeyWords.keywords.Concat(KeyWords.buildtIns).ToList());
            highlighter = new Highlighter(this);
            marker = new Thread(new ThreadStart(MarkerLoop)) {
                IsBackground = true
            };
            marker.Start();
        }

        public string GetText() {
            return new TextRange(CodeTextBox.Document.ContentStart, CodeTextBox.Document.ContentEnd).Text;
        }

        public void SetText(string text) {
            new TextRange(CodeTextBox.Document.ContentStart, CodeTextBox.Document.ContentEnd).Text = text;
            highlight = true;
        }

        private async void MarkerLoop() {

            while (highlighter == null)
                await Task.Delay(1000);

            while (true) {

                while (!highlight)
                    await Task.Delay(10);

                await Task.Delay(100);

                Dispatcher.Invoke(() => {
                    highlighter.HighlightAll();
                });

                highlight = false;
            }
        }

        private void CodeBoxTextChange(object sender, System.Windows.Controls.TextChangedEventArgs e) {

            if (LineNumsBox == null)
                return;

            int line_breaks = GetText().Split('\n').Length - 1;

            if (lines_count == line_breaks)
                return;

            lines_count = line_breaks;

            LineNumsBox.Text = "1\n";
            for (int i = 2; i <= lines_count; i++) {
                LineNumsBox.Text += i + "\n";
            }
        }

        private void HandleSuggestion(KeyEventArgs e) {
            string asString = e.Key.ToString();

            if (asString.Length == 1 && char.IsLetter(asString[0])) {
                typingWord += Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? asString[0] : char.ToLower(asString[0]);
                UpdateSuggestions(typingWord);
            } else if (suggestionBox != null && e.Key == Key.Down) {
                suggestionBox.GoDown();
                e.Handled = true;
            } else if (suggestionBox != null && e.Key == Key.Up) {
                suggestionBox.GoUp();
                e.Handled = true;
            } else if (suggestionBox != null && suggestionBox.selectedButton != null && e.Key == Key.Return) {
                string to_append = (string)suggestionBox.selectedButton.Content;
                to_append = to_append.Substring(typingWord.Length);
                CodeTextBox.CaretPosition.InsertTextInRun(to_append);
                CodeTextBox.CaretPosition = CodeTextBox.CaretPosition.GetPositionAtOffset(to_append.Length);
                typingWord = "";
                RemoveSuggestions();
                e.Handled = true;
            } else if (e.Key == Key.Tab) {
                if (suggestionBox != null) {
                    string to_append = (string)suggestionBox.sugButtons[0].Content;
                    to_append = to_append.Substring(typingWord.Length);
                    CodeTextBox.CaretPosition.InsertTextInRun(to_append);
                    CodeTextBox.CaretPosition = CodeTextBox.CaretPosition.GetPositionAtOffset(to_append.Length);
                } else {
                    CodeTextBox.CaretPosition.InsertTextInRun("    ");
                    CodeTextBox.CaretPosition = CodeTextBox.CaretPosition.GetPositionAtOffset(4);
                }
                typingWord = "";
                RemoveSuggestions();
                e.Handled = true;
            } else if (e.Key == Key.Back) {
                if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl)) {
                    typingWord = "";
                } else if (typingWord != "") {
                    typingWord = typingWord.Remove(typingWord.Length - 1);
                    UpdateSuggestions(typingWord);
                }
            } else {
                typingWord = "";
            }

            if (typingWord == "")
                RemoveSuggestions();
        }


        private void BoxKeyDown(object sender, KeyEventArgs e) {

            highlight = true;

            HandleSuggestion(e);

            if (e.Key == Key.Return) {
                TextPointer tp = CodeTextBox.CaretPosition.GetNextContextPosition(LogicalDirection.Backward);
                TextRange tr = new TextRange(tp, CodeTextBox.CaretPosition);
                if (tr.Text == ":") {
                    CodeTextBox.CaretPosition.InsertTextInRun("\n    ");
                    CodeTextBox.CaretPosition = CodeTextBox.CaretPosition.GetPositionAtOffset(4);
                    e.Handled = true;
                }
            }
        }

        public void UpdateSuggestions(string wordStart) {

            RemoveSuggestions();

            string[] matches = matcher.FindMatches(wordStart);
            if (matches.Length == 0)
                return;

            suggestionBox = new SuggestionBox();

            TextPointer caretPos = CodeTextBox.CaretPosition;
            int lines = new TextRange(CodeTextBox.Document.ContentStart, caretPos).Text.Split('\n').Length;
            int line_chars = new TextRange(caretPos.GetLineStartPosition(0), caretPos).Text.Length + 2;

            suggestionBox.Margin = new Thickness(line_chars * 8, 16 * lines, 0, 0);

            MainGrid.Children.Add(suggestionBox);
        }

        public void RemoveSuggestions() {
            if (suggestionBox != null)
                MainGrid.Children.Remove(suggestionBox);
            suggestionBox = null;
        }
    }

    public class Matcher {

        List<string> toCompareTos;
        public Matcher(List<string> to_compare_tos) {
            toCompareTos = to_compare_tos;
        }

        public string[] FindMatches(string inp) {
            List<string> matches = new List<string>();

            foreach (string toCompareTo in toCompareTos) {
                if (toCompareTo.StartsWith(inp) && toCompareTo.Length != inp.Length) {
                    matches.Add(toCompareTo);
                }
            }

            return matches.ToArray();
        }

        public void AddToCompare(string s) {
            toCompareTos.Add(s);
        }
    }
}
