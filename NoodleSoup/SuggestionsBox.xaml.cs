using Colors;
using Styles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NoodleSoup {
    public partial class SuggestionsBox : UserControl {
        public delegate void ClickedSuggestion(object sender, System.EventArgs e);
        public event ClickedSuggestion SuggestionClick;

        public List<Button> SugButtons = new List<Button>();
        public Button SelectedButton = null;
        private int SelectedButtonIdx = -1;
        private readonly SolidColorBrush SelectedBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        private readonly SolidColorBrush UnSelectedBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        private string TypingWord = "";
        private readonly string[] PossibleWords = KeyWords.keywords.Concat(KeyWords.buildtIns).ToArray();
        public bool IsOpen = true;

        public SuggestionsBox() {
            InitializeComponent();
            Close();
        }

        public void Close() {
            if (!IsOpen)
                return;
            MainPanel.Children.Clear();
            SugButtons.Clear();
            Visibility = Visibility.Collapsed;
            IsOpen = false;
        }

        private void UpdateTypingWord() {

            MainPanel.Children.Clear();
            SugButtons.Clear();

            foreach (string suggestion in PossibleWords) {

                if (!suggestion.StartsWith(TypingWord) || suggestion.Length == TypingWord.Length)
                    continue;


                Button sug = new Button {
                    Content = suggestion,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = UnSelectedBrush,
                    FontSize = 12,
                    FontFamily = new FontFamily("Cascadia Mono"),
                    Foreground = ColorDict.Get(suggestion),
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(1, 0, 1, 0),
                };
                sug.Click += (s, e) => SuggestionClick(s, e);

                SugButtons.Add(sug);

                MainPanel.Children.Add(sug);
            }

            if (MainPanel.Children.Count == 0)
                Close();
        }

        public void Update(string wordStart, int x, int y, bool showAll = false) {

            if (wordStart == "" && !showAll) {
                Close();
            } else {
                Margin = new Thickness(x, y, 0, 0);
                Open(wordStart);
            }
        }

        public void Open(string wordStart) {
            Visibility = Visibility.Visible;
            TypingWord = wordStart;
            IsOpen = true;
            SelectedButton = null;
            SelectedButtonIdx = -1;
            UpdateTypingWord();
        }

        public void GoDown() {
            if (SelectedButton != null)
                SelectedButton.Background = UnSelectedBrush;
            SelectedButtonIdx++;
            if (SelectedButtonIdx >= SugButtons.Count)
                SelectedButtonIdx = 0;
            SelectedButton = SugButtons[SelectedButtonIdx];
            SelectedButton.Background = SelectedBrush;
            MainScrollViewer.ScrollToVerticalOffset((SelectedButtonIdx - 6) * SelectedButton.ActualHeight);
        }

        public void GoUp() {
            if (SelectedButton != null)
                SelectedButton.Background = UnSelectedBrush;
            SelectedButtonIdx--;
            if (SelectedButtonIdx < 0)
                SelectedButtonIdx = SugButtons.Count - 1;
            SelectedButton = SugButtons[SelectedButtonIdx];
            SelectedButton.Background = SelectedBrush;
            MainScrollViewer.ScrollToVerticalOffset((SelectedButtonIdx - 6) * SelectedButton.ActualHeight);
        }

        public string GetSelectedText() {
            if (SelectedButton == null)
                return "";

            return (string) SelectedButton.Content;
        }
    }
}
