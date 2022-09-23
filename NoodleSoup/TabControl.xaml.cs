using System;
using System.Windows;
using System.Windows.Controls;

namespace NoodleSoup {
    public partial class TabControl : UserControl {

        int TotalWidth = 0;
        public delegate void UserChangesSelectionHandler(object sender, EventArgs e);
        public event UserChangesSelectionHandler OnUserChangesSelection;

        public delegate void UserRemoveTabHandler(object sender, EventArgs e);
        public event UserRemoveTabHandler UserWillRemoveTab;

        public TabControl() {
            InitializeComponent();
        }

        public void AddTab(string path) {

            TabItem tabItem = new TabItem(path) {
                Margin = new Thickness(TotalWidth, 0, 0, 0)
            };
            tabItem.closeButton.Click += CloseFileClick;
            tabItem.selectButton.Click += TabItemClick;

            MainGrid.Children.Add(tabItem);

            Select(tabItem);

            tabItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            tabItem.Arrange(new Rect(tabItem.DesiredSize));
            TotalWidth += (int) tabItem.ActualWidth;
        }

        public void RemoveTab(TabItem tab) {

            MainGrid.Children.Remove(tab);

            TotalWidth -= (int) tab.ActualWidth;

            int width = 0;
            foreach (TabItem tabItem in MainGrid.Children) {
                tabItem.Margin = new Thickness(width, 0, 0, 0);
                width += (int) tabItem.ActualWidth;
            }

            if (tab.isSelected && MainGrid.Children.Count > 0) {
                TabItem tabItem = (TabItem) MainGrid.Children[0];
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

            Button selectButton = (Button) sender;
            TabItem clickedItem = (TabItem) selectButton.Tag;

            if (clickedItem.isSelected)
                return;

            foreach (TabItem tabItem in MainGrid.Children) {
                if (tabItem == clickedItem) {
                    tabItem.Select();
                } else {
                    tabItem.Deselect();
                }
            }
            OnUserChangesSelection(clickedItem, new EventArgs());
        }

        private void CloseFileClick(object sender, RoutedEventArgs e) {
            UserWillRemoveTab(sender, e);
        }

        public bool Contains(string path) {
            foreach (TabItem tabItem in MainGrid.Children) {
                if (path == tabItem.Path)
                    return true;
            }
            return false;
        }
    }

    public class TabItem : Styles.TabItemButton {
        public string title;
        public string Path;
        public bool isSelected;

        public TabItem(string path) : base(System.IO.Path.GetFileName(path)) {
            Path = path;
        }

        public void Select() {
            Background = Styles.DirectoryButton.SelectedColor;
            isSelected = true;
        }

        public void Deselect() {
            Background = Styles.DirectoryButton.BackgroundColor;
            isSelected = false;
        }
    }

    public class SelectionChangeArgs : EventArgs {
        public string Status { get; private set; }
        public SelectionChangeArgs(string status) {
            Status = status;
        }
    }
}
