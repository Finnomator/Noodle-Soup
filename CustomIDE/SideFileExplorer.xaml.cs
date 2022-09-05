using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomIDE {


    public partial class SideFileExplorer : UserControl {

        public delegate void UserChangesSelectionHandler(object sender, EventArgs e);
        public event UserChangesSelectionHandler OnUserChangesSelection;
        public string DisplayedDir;

        public SideFileExplorer() {
            InitializeComponent();
        }

        public void OpenDir(string dirPath) {
            if (DisplayedDir == dirPath) {
                return;
            }
            DisplayedDir = dirPath;
            FileExplorer.Items.Clear();
            new DirectoryButton(dirPath, FileExplorer);
            PathBox.Text = dirPath;

            foreach (object item in FileExplorer.Items) {
                if (item.GetType() == typeof(DirectoryButton))
                    continue;
                FileButton fileButton = (FileButton)item;

                fileButton.Click += FileButtonClick;
            }
        }

        private void FileButtonClick(object sender, RoutedEventArgs e) {

            if (((FileButton)sender).isSelected)
                return;

            OnUserChangesSelection(sender, e);
        }

        public void Select(string path) {
            foreach (object o in FileExplorer.Items) {
                if (o.GetType() != typeof(FileButton))
                    continue;

                FileButton fileButton = (FileButton)o;

                if (fileButton.FilePath == path) {
                    fileButton.Select();
                } else
                    fileButton.Deselect();
            }
        }
    }

    public class DirectoryButton : Styles.DirectoryButton {
        bool IsExpanded;
        readonly List<object> Children = new List<object>();
        public int Depth;
        public string DirName;
        public string DirPath;
        public ListBox Box;

        public DirectoryButton(string dirName, DirectoryButton parent) : base() {
            DirName = dirName;
            DirPath = Path.Combine(parent.DirPath, DirName);
            Depth = parent.Depth + 1;
            Content = "> " + DirName;
            IsExpanded = false;
            Box = parent.Box;
            Indent(Depth * 10);
        }

        public DirectoryButton(string path, ListBox box) : base() {
            DirPath = path;
            Box = box;
            DirName = Path.GetFileName(DirPath);
            Depth = -1;
            Content = "V " + DirName;
            IsExpanded = true;
            AddChildrenToBox();
        }

        void AddChildrenToBox() {

            foreach (string path in Directory.GetDirectories(DirPath))
                Children.Add(new DirectoryButton(Path.GetFileName(path), this));

            foreach (string path in Directory.GetFiles(DirPath))
                Children.Add(new FileButton(Path.GetFileName(path), this));

            for (int i = 0; i < Children.Count; ++i) {
                Box.Items.Insert(Box.Items.IndexOf(this) + i + 1, Children[i]);
            }
        }

        void RemoveChildrenFromBox() {
            for (int i = 0; i < Children.Count; ++i) {
                Box.Items.Remove(Children[i]);
            }
            Children.Clear();
        }

        public void Indent(int indent) {
            Margin = new Thickness(indent, 0, 0, 0);
        }

        protected override void OnClick() {
            base.OnClick();

            IsExpanded = !IsExpanded;

            if (IsExpanded) {
                Content = "V " + DirName;
                AddChildrenToBox();
            } else {
                Content = "> " + DirName;
                RemoveChildrenFromBox();
            }
        }
    }

    public class FileButton : Styles.FileButton {
        public string FileName;
        public string FileExt;
        public string FilePath;
        readonly int Depth;
        public bool isSelected;

        public FileButton(string fileName, DirectoryButton parent) : base() {
            FileName = fileName;
            FileExt = Path.GetExtension(FileName);
            FilePath = Path.Combine(parent.DirPath, FileName);
            Depth = parent.Depth + 1;
            Content = fileName;
            Indent(Depth * 10);
        }

        public void Select() {
            isSelected = true;
            Background = Styles.DirectoryButton.SelectedColor;
        }

        public void Deselect() {
            isSelected = false;
            Background = Styles.DirectoryButton.BackgroundColor;
        }

        public void Indent(int indent) {
            Margin = new Thickness(indent, 0, 0, 0);
        }
    }
}
