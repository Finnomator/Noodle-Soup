using Microsoft.Win32;
using NoodleSoup.Properties;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NoodleSoup {

    public partial class MainWindow : Window {

        private string CurrentFilePath;
        private Options options;
        private bool RunningCommand {
            set {
                StopBut.IsEnabled = value;
            }
            get {
                return StopBut.IsEnabled;
            }
        }
        private readonly string tempFilePath = Path.GetFullPath("Temp.py");

        public MainWindow() {
            InitializeComponent();

            RunningCommand = false;

            options = new Options();
            Settings.Default.PythonInstalled = options.IsPythonInstalled();
            Settings.Default.AmpyInstalled = options.IsAmpyInstalled();
            Settings.Default.Save();

            CurrentFilePath = Path.GetFullPath(Settings.Default.LastOpenedFilePath);

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            FileExplorer.OpenDir(Directory.GetParent(CurrentFilePath).FullName);

            TabControler.OnUserChangesSelection += TabControlUserChange;
            TabControler.UserWillRemoveTab += TabControlRmTab;
            FileExplorer.OnUserChangesSelection += FileSelectionChange;
            GoodTextBox.HotkeyPressed += GoodTextBox_HotkeyPressed;
            Terminal.OnCmdFinished += Terminal_OnCmdFinished;

            OpenFile(CurrentFilePath);
        }

        private void Terminal_OnCmdFinished(object sender, EventArgs e) {
            RunningCommand = false;
        }

        private void GoodTextBox_HotkeyPressed(object sender, HotkeyEventArgs e) {
            if (e.PressedKeys.Length == 2) {
                if (e.PressedKeys.Contains(Key.LeftCtrl) && e.PressedKeys.Contains(Key.S))
                    SaveFile();
            }
        }


        private void TabControlRmTab(object sender, EventArgs e) {

            TabItem tab = (TabItem) ((Button) sender).Tag;

            if (SaveFile())
                TabControler.RemoveTab(tab);

            if (TabControler.MainGrid.Children.Count == 0)
                OpenFile(tempFilePath);
        }

        private void TabControlUserChange(object sender, EventArgs e) {
            TabItem tabItem = (TabItem) sender;
            if (SaveFile())
                OpenFile(tabItem.Path);
        }

        private void FileSelectionChange(object sender, EventArgs e) {
            FileButton fileButton = (FileButton) sender;
            if (SaveFile())
                OpenFile(fileButton.FilePath);
        }

        private void OpenFile(string path) {
            if (path == null)
                return;

            CurrentFilePath = path;

            try {
                GoodTextBox.SetText(File.ReadAllText(CurrentFilePath));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TabControler.Select(CurrentFilePath);
            FileExplorer.Select(CurrentFilePath);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e) {

            if (!SaveFile())
                return;

            if (SelectFile() == null)
                return;

            OpenFile(CurrentFilePath);

            FileExplorer.OpenDir(Directory.GetParent(CurrentFilePath).FullName);
        }

        private string SelectFile() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if ((bool) !openFileDialog.ShowDialog())
                return null;
            CurrentFilePath = openFileDialog.FileName;
            return CurrentFilePath;
        }

        private bool SaveFile() {
            try {
                File.WriteAllText(CurrentFilePath, GoodTextBox.GetText());
            } catch (Exception ex) {
                MessageBoxResult boxResult = MessageBox.Show(ex.Message + "\nContinue without saving?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return boxResult == MessageBoxResult.Yes;
            }
            return true;
        }

        private void SaveClick(object sender, RoutedEventArgs e) {
            SaveFile();
        }


        private void SaveAsClick(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if ((bool) !saveFileDialog.ShowDialog())
                return;

            CurrentFilePath = saveFileDialog.FileName;
            SaveFile();
        }

        private void RunTerminalCommand(string command) {
            RunningCommand = true;
            Terminal.Run(command);
        }

        private void RunScriptClick(object sender, RoutedEventArgs e) {
            SaveFile();

            if (!options.IsCOMPortAvailable(Settings.Default.SelectedCOMPort)) {
                Settings.Default.SelectedCOMPort = -1;
                MessageBox.Show("Select a COM port first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else if (!Settings.Default.PythonInstalled) {
                MessageBox.Show("Install Python first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else if (!Settings.Default.AmpyInstalled) {
                MessageBox.Show("Install adafruit-ampy first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RunTerminalCommand("ampy --port COM" + Settings.Default.SelectedCOMPort + " run " + CurrentFilePath);
        }

        private void StopScriptClick(object sender, RoutedEventArgs e) {
            Terminal.Stop();
        }

        private void OptionsClick(object sender, RoutedEventArgs e) {
            options = new Options();
            options.Show();
        }

        private void WindowCloses(object sender, EventArgs e) {
            Settings.Default.LastOpenedFilePath = CurrentFilePath;
            Settings.Default.Save();
            options.Close();
            Application.Current.Shutdown();
        }

        private void MinimizeClick(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void CloseClick(object sender, RoutedEventArgs e) {
            if (!SaveFile())
                return;
            Close();
        }

        private void RunPythonScriptClick(object sender, RoutedEventArgs e) {
            SaveFile();
            if (!Settings.Default.PythonInstalled) {
                MessageBox.Show("Install Python first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            RunTerminalCommand("python " + CurrentFilePath);
        }

        private void MaximiseClick(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Normal) {
                WindowState = WindowState.Maximized;
            } else {
                WindowState = WindowState.Normal;
            }
        }

        private void Window_LeftMouseDowm(object sender, MouseButtonEventArgs e) {
            Point mouse_pos = Mouse.GetPosition(TitleBar);
            if (mouse_pos.Y < TitleBar.ActualHeight && mouse_pos.X < TitleBar.ActualWidth) {
                if (WindowState != WindowState.Normal) {
                    WindowState = WindowState.Normal;
                    Point mousePos = Mouse.GetPosition(this);
                    Top = mousePos.Y - ToolBar.ActualHeight / 2;
                    Left = mousePos.X - ToolBar.ActualWidth / 2;
                }
                DragMove();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (WindowState == WindowState.Maximized)
                EdgeBorder.BorderThickness = new Thickness(8);
            else
                EdgeBorder.BorderThickness = new Thickness(3);

        }

        private void Window_StateChange(object sender, EventArgs e) {
            if (WindowState == WindowState.Normal)
                MaximizeButton.Content = "🗖";
            else
                MaximizeButton.Content = "🗗";
        }
    }
}
