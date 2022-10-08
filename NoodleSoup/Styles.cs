using Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Styles {

    public class CleanButton : Button {
        public CleanButton() : base() {
            Template = FindResource("CleanButton") as ControlTemplate;
        }

    }
    public class FileButton : DirectoryButton {
        public FileButton() : base() { }
    }

    public class DirectoryButton : CleanButton {

        public static Brush BackgroundColor;
        public static Brush SelectedColor;

        public DirectoryButton() : base() {
            FontSize = 10;
            HorizontalContentAlignment = HorizontalAlignment.Left;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            BackgroundColor = Background;
            SelectedColor = new SolidColorBrush(Color.FromRgb(120, 120, 120));
        }
    }

    public class TabItemButton : Grid {

        public CleanButton selectButton;
        public CleanButton closeButton;

        public TabItemButton(string title) : base() {

            HorizontalAlignment = HorizontalAlignment.Left;
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition());

            selectButton = new CleanButton {
                Content = title,
                Tag = this,
                FontSize = 11,
                BorderThickness = new Thickness(0),
            };

            closeButton = new CleanButton {
                Content = "🗙",
                Tag = this,
                FontSize = 11,
                BorderThickness = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            selectButton.SetValue(Grid.ColumnProperty, 0);
            closeButton.SetValue(Grid.ColumnProperty, 1);

            Children.Add(closeButton);
            Children.Add(selectButton);
        }
    }
    /*
    public class SuggestionBox : Border {

        public delegate void ClickedSuggestion(object sender, System.EventArgs e);
        public event ClickedSuggestion SuggestionClick;

        public List<Button> sugButtons = new List<Button>();
        public Button selectedButton = null;
        int selectedButtonIdx = -1;
        SolidColorBrush selectedBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        SolidColorBrush unselectedBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        SolidColorBrush borderBrush = Brushes.Gray;
        StackPanel mainPanel = new StackPanel();
        ScrollViewer scrollViewer = new ScrollViewer();
        public string typingWord = "";
        public string[] possibleWords = KeyWords.keywords.Concat(KeyWords.buildtIns).ToArray();
        public bool isOpen;

        public SuggestionBox() : base() {

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;

            BorderBrush = borderBrush;
            BorderThickness = new Thickness(1, 1, 1, 1);

            MaxWidth = 200;
            MaxHeight = 200;

            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            scrollViewer.Content = mainPanel;
            Child = scrollViewer;

            Close();
        }

        public void Close() {
            mainPanel.Children.Clear();
            sugButtons.Clear();
            Visibility = Visibility.Hidden;
            isOpen = false;
        }

        public IEnumerable<string> FindMatches(string wordStart) {

            foreach (string word in possibleWords) {
                if (word.StartsWith(wordStart) && word.Length != wordStart.Length) {
                    yield return word;
                }
            }
        }

        private void UpdateTypingWord() {

            mainPanel.Children.Clear();
            sugButtons.Clear();

            foreach (string suggestion in FindMatches(typingWord)) {

                Button sug = new Button {
                    Content = suggestion,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = unselectedBrush,
                    FontSize = 12,
                    FontFamily = new FontFamily("Cascadia Mono"),
                    Foreground = ColorDict.Get(suggestion),
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(1, 0, 1, 0),
                };

                sug.Click += SugButtonClick;

                sugButtons.Add(sug);

                mainPanel.Children.Add(sug);
            }

            if (mainPanel.Children.Count == 0)
                Close();
        }

        private void SugButtonClick(object sender, System.EventArgs e) {
            SuggestionClick(sender, e);
        }

        public void Update(string wordStart, int x, int y, bool showAll = false) {

            if (wordStart == typingWord)
                return;

            Margin = new Thickness(x, y, 0, 0);

            if (wordStart == "" && !showAll)
                Close();
            else
                Open(wordStart);
        }

        public void Open(string wordStart) {
            Visibility = Visibility.Visible;
            typingWord = wordStart;
            isOpen = true;
            selectedButton = null;
            selectedButtonIdx = -1;
            UpdateTypingWord();
        }

        public void GoDown() {
            if (selectedButton != null)
                selectedButton.Background = unselectedBrush;
            selectedButtonIdx++;
            if (selectedButtonIdx >= sugButtons.Count)
                selectedButtonIdx = 0;
            selectedButton = sugButtons[selectedButtonIdx];
            selectedButton.Background = selectedBrush;
            scrollViewer.ScrollToVerticalOffset((selectedButtonIdx - 6) * selectedButton.ActualHeight);
        }

        public void GoUp() {
            if (selectedButton != null)
                selectedButton.Background = unselectedBrush;
            selectedButtonIdx--;
            if (selectedButtonIdx < 0)
                selectedButtonIdx = sugButtons.Count - 1;
            selectedButton = sugButtons[selectedButtonIdx];
            selectedButton.Background = selectedBrush;
            scrollViewer.ScrollToVerticalOffset((selectedButtonIdx - 6) * selectedButton.ActualHeight);
        }
    }*/


    public class ComPortItem : ComboBoxItem {
        public int Port;
        public ComPortItem(string content, int port) : base() {
            Content = content;
            Port = port;
        }

        public ComPortItem() {
            Port = -1;
            Content = "None";
        }
    }
}