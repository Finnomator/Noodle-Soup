using Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CustomIDE {


    public partial class SideFileExplorer : UserControl {

        public delegate void UserChangesSelectionHandler(object sender, EventArgs e);
        public event UserChangesSelectionHandler OnUserChangesSelection;
        public DirectoryButton DisplayedDir;
        public DirectoryButton SelectedDirectory;
        public FileButton SelectedFile;

        public SideFileExplorer() {
            InitializeComponent();
        }

        public void OpenDir(string dirPath) {
            if (DisplayedDir != null && DisplayedDir.DirPath == dirPath)
                return;
            FileExplorer.Items.Clear();

            DisplayedDir = new DirectoryButton(dirPath, FileExplorer, FileButtonClick, DirectoryButtonClick); ;

            SelectedDirectory = DisplayedDir;

            PathBox.Text = dirPath;
        }

        public void Refresh() {
            DisplayedDir.Refresh();
        }

        private void FileButtonClick(object sender, EventArgs e) {

            if (((FileButton) sender).isSelected)
                return;

            SelectedFile = (FileButton) sender;
            OnUserChangesSelection(sender, e);
        }

        private void DirectoryButtonClick(object sender, EventArgs e) {

            if (((DirectoryButton) sender).IsExpanded)
                return;

            SelectedDirectory = (DirectoryButton) sender;
        }

        public void Select(string path) {
            foreach (object o in FileExplorer.Items) {
                if (o.GetType() != typeof(FileButton))
                    continue;

                FileButton fileButton = (FileButton) o;

                if (fileButton.FilePath == path) {
                    fileButton.Select();
                    SelectedFile = fileButton;
                } else
                    fileButton.Deselect();
            }
        }

        private void RefreshClick(object sender, RoutedEventArgs e) {
            Refresh();
        }

        private new void RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true;
        }

        private void AddFile_Click(object sender, RoutedEventArgs e) {

            string newFilePath = SelectedDirectory.DirPath + "/TestFile.py";

            if (File.Exists(newFilePath)) {
                MessageBox.Show("File already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try {
                File.Create(newFilePath);
            } catch (Exception ex) {
                MessageBox.Show("Exception: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Refresh();
        }

        private void AddDir_Click(object sender, RoutedEventArgs e) {

            string newDirPath = SelectedDirectory.DirPath + "/TestDir";

            if (Directory.Exists(newDirPath)) {
                MessageBox.Show("Directory already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try {
                Directory.CreateDirectory(newDirPath);
            } catch (Exception ex) {
                MessageBox.Show("Exception: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Refresh();
        }
    }

    public class DirectoryButton : Styles.DirectoryButton {
        public bool IsExpanded;
        private List<DirectoryButton> ChildrenDirs = new List<DirectoryButton>();
        private List<FileButton> ChildrenFiles = new List<FileButton>();
        public int Depth;
        public string DirName;
        public string DirPath;
        public ListBox Box;
        public Action<object, EventArgs> FileClickPointer;
        private Action<object, EventArgs> DirClickPointer;

        public DirectoryButton(string dirName, DirectoryButton parent) : base() {
            DirName = dirName;
            DirPath = Path.Combine(parent.DirPath, DirName);
            Depth = parent.Depth + 1;
            Content = "> " + DirName;
            IsExpanded = false;
            Box = parent.Box;
            FileClickPointer = parent.FileClickPointer;
            DirClickPointer = parent.DirClickPointer;
            Click += parent.DirClickPointer.Invoke;
            Indent(Depth * 10);
        }

        public DirectoryButton(string path, ListBox box, Action<object, EventArgs> fileClickPointer, Action<object, EventArgs> dirClickPointer) : base() {
            DirPath = path;
            Box = box;
            FileClickPointer = fileClickPointer;
            DirClickPointer = dirClickPointer;
            DirName = Path.GetFileName(DirPath);
            Depth = -1;
            Content = "V " + DirName;
            IsExpanded = true;
            AddChildrenToBox();
        }

        public void Refresh() {
            if (!IsExpanded)
                return;

            string[] dirs = Directory.GetDirectories(DirPath);
            string[] files = Directory.GetFiles(DirPath);
            string[] childrenDirs = (from child in ChildrenDirs select child.DirPath).ToArray();
            string[] childrenFiles = (from child in ChildrenFiles select child.FilePath).ToArray();
            List<DirectoryButton> dirsToRemove = new List<DirectoryButton>();
            List<FileButton> filesToRemvoe = new List<FileButton>();
            int thisIdx = Box.Items.IndexOf(this);


            for (int i = 0; i < dirs.Length; ++i) {
                string dir = dirs[i];
                if (childrenDirs.Contains(dir))
                    continue;
                DirectoryButton directoryButton = new DirectoryButton(Path.GetFileName(dir), this);
                ChildrenDirs.Add(directoryButton);
                Box.Items.Insert(thisIdx + i + 1, directoryButton);
            }

            foreach (DirectoryButton child in ChildrenDirs) {
                if (!dirs.Contains(child.DirPath))
                    dirsToRemove.Add(child);
            }

            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                if (childrenFiles.Contains(file))
                    continue;
                FileButton fileButton = new FileButton(Path.GetFileName(file), this);
                ChildrenFiles.Add(fileButton);
                Box.Items.Insert(thisIdx + i + 1 + dirs.Length, fileButton);
            }

            foreach (FileButton child in ChildrenFiles) {
                if (!files.Contains(child.FilePath))
                    filesToRemvoe.Add(child);
            }

            foreach (DirectoryButton dirButton in dirsToRemove) {
                ChildrenDirs.Remove(dirButton);
                Box.Items.Remove(dirButton);
            }

            foreach (FileButton button in filesToRemvoe) {
                ChildrenFiles.Remove(button);
                Box.Items.Remove(button);
            }


            foreach (DirectoryButton child in ChildrenDirs) {
                child.Refresh();
            }
        }

        private void AddChildrenToBox() {

            int thisIdx = Box.Items.IndexOf(this);

            foreach (string path in Directory.GetDirectories(DirPath))
                ChildrenDirs.Add(new DirectoryButton(Path.GetFileName(path), this));

            foreach (string path in Directory.GetFiles(DirPath))
                ChildrenFiles.Add(new FileButton(Path.GetFileName(path), this));

            for (int i = 0; i < ChildrenDirs.Count; ++i)
                Box.Items.Insert(thisIdx + i + 1, ChildrenDirs[i]);

            for (int i = 0; i < ChildrenFiles.Count; ++i)
                Box.Items.Insert(thisIdx + i + 1 + ChildrenDirs.Count, ChildrenFiles[i]);
        }

        private void RemoveChildrenFromBox() {
            foreach (DirectoryButton child in ChildrenDirs)
                Box.Items.Remove(child);

            foreach (FileButton child in ChildrenFiles)
                Box.Items.Remove(child);

            ChildrenDirs.Clear();
            ChildrenFiles.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Indent(int indent) {
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
            Click += parent.FileClickPointer.Invoke;
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

        private void Indent(int indent) {
            Margin = new Thickness(indent, 0, 0, 0);
        }
    }
}
