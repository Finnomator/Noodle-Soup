using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CustomIDE {
    public partial class TabControl : UserControl {

        int TotalWidth = 0;
        public delegate void UserChangesSelectionHandler(object sender, EventArgs e);
        public event UserChangesSelectionHandler OnUserChangesSelection;

        public TabControl() {
            InitializeComponent();
        }

        public void AddTab(string path) {

            TabItem tabItem = new TabItem(path) {
                Margin = new Thickness(TotalWidth, 0, 0, 0)
            };
            ((TabItemContent)tabItem.Content).closeButton.Click += CloseFileClick;
            tabItem.Click += TabItemClick;

            MainGrid.Children.Add(tabItem);

            Select(tabItem);

            tabItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            tabItem.Arrange(new Rect(tabItem.DesiredSize));
            TotalWidth += (int)tabItem.ActualWidth;
        }

        public void RemoveTab(TabItem tab) {

            MainGrid.Children.Remove(tab);

            TotalWidth -= (int)tab.ActualWidth;

            int width = 0;
            foreach (TabItem tabItem in MainGrid.Children) {
                tabItem.Margin = new Thickness(width, 0, 0, 0);
                width += (int)tabItem.ActualWidth;
            }

            if (MainGrid.Children.Count > 0) {
                TabItem tabItem = (TabItem)MainGrid.Children[0];
                OnUserChangesSelection(tabItem, new EventArgs());
                Select(tabItem);
            }

        }

        public void Select(TabItem one) {
            Select(one.Path);
        }

        public void Select(string path) {
            bool found = false;
            foreach (TabItem tabItem in MainGrid.Children) {
                if (path == tabItem.Path) {
                    if (!tabItem.isSelected) {
                        tabItem.Select();
                    }
                    found = true;
                } else {
                    tabItem.Deselect();
                }
            }
            if (!found)
                AddTab(path);
        }

        private void TabItemClick(object sender, RoutedEventArgs e) {
            TabItem clickedItem = (TabItem)sender;

            if (clickedItem.isSelected)
                return;

            foreach (TabItem tabItem in MainGrid.Children) {
                if (tabItem == clickedItem) {
                    tabItem.Select();
                } else {
                    tabItem.Deselect();
                }
            }
            OnUserChangesSelection((TabItem)sender, new EventArgs());
        }

        private void CloseFileClick(object sender, RoutedEventArgs e) {
            TabItemContent tabItemContent = (TabItemContent)((Button)sender).Parent;
            TabItem tab = (TabItem)tabItemContent.Parent;
            RemoveTab(tab);
        }

        public bool Contains(string path) {
            foreach (TabItem tabItem in MainGrid.Children) {
                if (path == tabItem.Path)
                    return true;
            }
            return false;
        }
    }

    public class TabItem : Button {
        public string title;
        public string Path;
        public bool isSelected;

        public TabItem(string path) : base() {
            Path = path;
            title = System.IO.Path.GetFileName(Path);

            Content = new TabItemContent(title);

            Padding = new Thickness(1, 0, 1, 0);
            HorizontalAlignment = HorizontalAlignment.Left;
        }

        public void Select() {
            BorderThickness = new Thickness(1, 1, 1, 0);
            Background = Styles.DirectoryButton.SelectedColor;
            isSelected = true;
        }

        public void Deselect() {
            BorderThickness = new Thickness(1, 1, 1, 1);
            Background = Styles.DirectoryButton.BackgroundColor;
            isSelected = false;
        }
    }

    public class TabItemContent : Grid {

        public Button closeButton = new Button();

        public TabItemContent(string content) : base() {

            ColumnDefinition col1 = new ColumnDefinition();
            ColumnDefinition col2 = new ColumnDefinition();

            col1.Width = GridLength.Auto;
            col2.Width = GridLength.Auto;

            ColumnDefinitions.Add(col1);
            ColumnDefinitions.Add(col2);

            Label l = new Label {
                Padding = new Thickness(0, 0, 1, 0),
                FontSize = 10,
                FontFamily = new FontFamily("Cascadia Mono"),
                Content = content,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            };

            closeButton.Content = "[X]";
            closeButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            closeButton.Padding = new Thickness(1, 0, 0, 0);
            closeButton.BorderThickness = new Thickness(0, 0, 0, 0);
            closeButton.FontSize = 10;
            closeButton.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

            SetColumn(l, 0);
            SetColumn(closeButton, 1);

            Children.Add(l);
            Children.Add(closeButton);
        }
    }

    public class SelectionChangeArgs : EventArgs {
        public string Status { get; private set; }
        public SelectionChangeArgs(string status) {
            Status = status;
        }
    }
}
