using Colors;
using CustomIDE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Styles {
    public class CodyButton : Button {
        public CodyButton() : base() { }
    }

    public class FileButton : DirectoryButton {
        public FileButton() : base() { }
    }

    public class DirectoryButton : Button {

        public static Brush BackgroundColor;
        public static Brush SelectedColor;

        public DirectoryButton() : base() {
            Margin = new Thickness(0, 0, 0, 0);
            BorderThickness = new Thickness(0, 0, 0, 0);
            Padding = new Thickness(0, 0, 0, 0);
            FontSize = 10;
            FontFamily = new FontFamily("Cascadia Mono");
            HorizontalContentAlignment = HorizontalAlignment.Left;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            BorderThickness = new Thickness(0, 0, 0, 0);
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            BackgroundColor = Background;
            SelectedColor = new SolidColorBrush(Color.FromRgb(120, 120, 120));
        }
    }

    public class TabItemButton : Grid {

        public Button selectButton;
        public Button closeButton;

        public TabItemButton(string title) : base() {

            HorizontalAlignment = HorizontalAlignment.Left;
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition());

            selectButton = new Button {
                Content = title,
                Background = Brushes.Transparent,
                Tag = this,
                Foreground = Brushes.White,
                Padding = new Thickness(1, 0, 3, 0),
                BorderThickness = new Thickness(0, 0, 0, 0),
                FontSize = 10,
            };

            closeButton = new Button {
                Content = "🗙",
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0, 2, 0, 2),
                FontSize = 9,
                Background = null,
                Tag = this,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            selectButton.MouseEnter += SelectButton_MouseEnter;
            selectButton.MouseLeave += SelectButton_MouseLeave;

            selectButton.SetValue(Grid.ColumnProperty, 0);
            closeButton.SetValue(Grid.ColumnProperty, 1);

            Children.Add(closeButton);
            Children.Add(selectButton);

        }

        private void SelectButton_MouseLeave(object sender, MouseEventArgs e) {
        }

        private void SelectButton_MouseEnter(object sender, MouseEventArgs e) {
        }
    }

    public class SuggestionBox : Border {

        public delegate void ClickedSuggestion(object sender, System.EventArgs e);
        public event ClickedSuggestion SuggestionClick;

        public List<Button> sugButtons = new List<Button>();
        public Button selectedButton = null;
        int selectedButtonIdx = -1;
        SolidColorBrush selectedBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        SolidColorBrush unselectedBrush = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        SolidColorBrush borderBrush = Brushes.Gray;
        Grid mainGrid = new Grid();
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

            scrollViewer.Content = mainGrid;
            Child = scrollViewer;

            Close();
        }

        public void Close() {
            mainGrid.Children.Clear();
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

            mainGrid.Children.Clear();
            mainGrid.RowDefinitions.Clear();
            sugButtons.Clear();

            foreach (string suggestion in FindMatches(typingWord)) {
                mainGrid.RowDefinitions.Add(new RowDefinition());

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

                sug.SetValue(Grid.RowProperty, mainGrid.RowDefinitions.Count - 1);

                sugButtons.Add(sug);

                mainGrid.Children.Add(sug);
            }

            if (mainGrid.Children.Count == 0)
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
    }


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

    public class FileContextMenu : Grid {

        public delegate void Click(object sender, FileContextEventArgs e);
        public event Click OnClick;

        private ContextButton[] contextButtons;

        public class ContextButton : Button {

            public FileOption Action;

            public ContextButton(FileOption fileOption) : base() {

                Action = fileOption;

                HorizontalAlignment = HorizontalAlignment.Stretch;
                VerticalAlignment = VerticalAlignment.Stretch;
                BorderThickness = new Thickness(0);
                Padding = new Thickness(2, 0, 2, 0);

                Background = Brushes.DarkGray;
                Foreground = Brushes.White;

                Content = fileOption.ToString();
                FontSize = 10;
                HorizontalContentAlignment = HorizontalAlignment.Left;
            }
        }

        public class FileContextEventArgs : EventArgs {
            public FileOption action { get; private set; }

            public FileContextEventArgs(FileOption action) {
                this.action = action;
            }
        }

        public FileContextMenu() : base() {

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;

            ContextButton renameButton = new ContextButton(FileOption.Rename);
            ContextButton cutButton = new ContextButton(FileOption.Cut);
            ContextButton copyButton = new ContextButton(FileOption.Copy);
            ContextButton pasteButton = new ContextButton(FileOption.Paste);
            ContextButton deleteButton = new ContextButton(FileOption.Delete);
            contextButtons = new ContextButton[] { renameButton, cutButton, copyButton, pasteButton, deleteButton };

            renameButton.Click += Button_Click;
            cutButton.Click += Button_Click;
            copyButton.Click += Button_Click;
            pasteButton.Click += Button_Click;
            deleteButton.Click += Button_Click;

            SetZIndex(this, 1);

            for (int i = 0; i < contextButtons.Length; i++) {
                ContextButton contextButton = contextButtons[i];
                RowDefinitions.Add(new RowDefinition());
                SetRow(contextButton, i);
                Children.Add(contextButton);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            OnClick(sender, new FileContextEventArgs(((ContextButton) sender).Action));
        }
    }

    public enum FileOption {
        Rename,
        Cut,
        Copy,
        Paste,
        Delete,
    }
}