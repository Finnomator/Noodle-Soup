using System;
using System.Windows;
using System.Windows.Controls;

namespace NoodleSoup {
    public partial class TabControl : UserControl {

        public delegate void UserChangesSelectionHandler(object sender, EventArgs e);
        public event UserChangesSelectionHandler OnUserChangesSelection;

        public delegate void UserRemoveTabHandler(object sender, EventArgs e);
        public event UserRemoveTabHandler UserWillRemoveTab;

        public TabControl() {
            InitializeComponent();
        }

        public void AddTab(string path) {

            TabItem tabItem = new TabItem(path);
            tabItem.closeButton.Click += CloseFileClick;
            tabItem.selectButton.Click += TabItemClick;

            MainPanel.Children.Add(tabItem);

            Select(tabItem);
        }

        public void RemoveTab(TabItem tab) {

            MainPanel.Children.Remove(tab);

            if (tab.isSelected && MainPanel.Children.Count > 0) {
                TabItem tabItem = (TabItem) MainPanel.Children[0];
                OnUserChangesSelection(tabItem, new EventArgs());
                Select(tabItem);
            }
        }

        public void Select(TabItem one) {
            Select(one.Path);
        }

        public void Select(string path) {
            bool found = false;
            foreach (TabItem tabItem in MainPanel.Children) {
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

            foreach (TabItem tabItem in MainPanel.Children) {
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
            foreach (TabItem tabItem in MainPanel.Children) {
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
