using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using CustomIDE.Properties;

namespace CustomIDE {

    public partial class MainWindow : Window {

        private string current_file_path;
        private Process CodeRunner;
        private Options options;
        private bool runningCommand = false;
        private readonly ProcessStartInfo startInfo = new ProcessStartInfo {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true
        };
        private string tempFilePath = Path.GetFullPath("Temp.py");

        public MainWindow() {
            InitializeComponent();
            options = new Options();
            Settings.Default.PythonInstalled = options.IsPythonInstalled();
            Settings.Default.AmpyInstalled = options.IsAmpyInstalled();
            Settings.Default.Save();

            current_file_path = Path.GetFullPath(Settings.Default.LastOpenedFilePath);

            FileExplorer.OpenDir(Directory.GetParent(current_file_path).FullName);

            TabControler.OnUserChangesSelection += TabControlUserChange;
            TabControler.UserWillRemoveTab += TabControlRmTab;
            FileExplorer.OnUserChangesSelection += FileSelectionChange;
            GoodTextBox.HotkeyPressed += GoodTextBox_HotkeyPressed;


            OpenFile(current_file_path);
        }

        private void GoodTextBox_HotkeyPressed(object sender, HotkeyEventArgs e) {
            if (e.PressedKeys.Length == 2) {
                if (e.PressedKeys.Contains(Key.LeftCtrl) && e.PressedKeys.Contains(Key.S))
                    SaveFile();
            }
        }


        private void TabControlRmTab(object sender, EventArgs e) {

            TabItem tab = ((TabItem) ((Button) sender).Tag);

            if (SaveFile())
                TabControler.RemoveTab(tab);

            if (TabControler.MainGrid.Children.Count == 0) {
                OpenFile(tempFilePath);
            }
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

            current_file_path = path;

            try {
                GoodTextBox.SetText(File.ReadAllText(current_file_path));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TabControler.Select(current_file_path);
            FileExplorer.Select(current_file_path);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e) {

            if (!SaveFile())
                return;

            if (SelectFile() == null)
                return;

            OpenFile(current_file_path);

            FileExplorer.OpenDir(Directory.GetParent(current_file_path).FullName);
        }

        private string SelectFile() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if ((bool) !openFileDialog.ShowDialog())
                return null;
            current_file_path = openFileDialog.FileName;
            return current_file_path;
        }

        private bool SaveFile() {
            try {
                File.WriteAllText(current_file_path, GoodTextBox.GetText());
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

            current_file_path = saveFileDialog.FileName;
            SaveFile();
        }

        private void RunTerminalCommand(string fileName, string args) {
            OutputBox.Text += "Running \"" + fileName + (args == "" ? "" : " " + args) + "\" ...\n";
            StopBut.IsEnabled = true;
            runningCommand = true;

            CodeRunner = new Process {
                StartInfo = startInfo
            };

            CodeRunner.StartInfo.FileName = fileName;
            CodeRunner.StartInfo.Arguments = args;

            CodeRunner.OutputDataReceived += new DataReceivedEventHandler((s, e) => {
                Dispatcher.Invoke(() => {
                    if (e.Data == null) {
                        runningCommand = false;
                        StopBut.IsEnabled = false;
                        OutputBox.Text += "[Done]\n";
                    } else {
                        OutputBox.Text += "[OUT] " + e.Data + "\n";
                    }
                });
            });
            CodeRunner.ErrorDataReceived += new DataReceivedEventHandler((s, e) => {
                Dispatcher.Invoke(() => {
                    if (e.Data != null)
                        OutputBox.Text += e.Data + "\n";
                });
            });

            try {
                CodeRunner.Start();
            } catch {
                OutputBox.Text += "Command \"" + fileName + "\" not found.\n[Done]\n";
                runningCommand = false;
                StopBut.IsEnabled = false;
                return;
            }
            CodeRunner.BeginOutputReadLine();
            CodeRunner.BeginErrorReadLine();
        }

        private void StopTerminalCommand() {
            CodeRunner.Kill();
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

            RunTerminalCommand("ampy", "--port COM" + Settings.Default.SelectedCOMPort + " run " + current_file_path);
        }

        private void StopScriptClick(object sender, RoutedEventArgs e) {
            StopTerminalCommand();
        }

        private void OptionsClick(object sender, RoutedEventArgs e) {
            options = new Options();
            options.Show();
        }

        private void WindowCloses(object sender, EventArgs e) {
            Settings.Default.LastOpenedFilePath = current_file_path;
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
            RunTerminalCommand("python", current_file_path);
        }

        private void MaximiseClick(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
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

        private void InputKeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Return)
                return;

            string input = InputBox.Text;

            InputBox.Text = "";

            OutputBox.Text += "[IN]  " + input + "\n";

            if (!runningCommand) {
                input = input.Trim();
                string fileName = input.Split(' ')[0];
                string args = "";
                if (fileName.Length < input.Length)
                    args = input.Substring(fileName.Length);

                RunTerminalCommand(fileName, args);
            } else {
                CodeRunner.StandardInput.WriteLine(input);
            }
        }

        private void OutputTextChange(object sender, TextChangedEventArgs e) {
            OutputBox.ScrollToEnd();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (WindowState == WindowState.Maximized) {
                EdgeBorder.BorderThickness = new Thickness(8);
            } else {
                EdgeBorder.BorderThickness = new Thickness(3);
            }
        }
    }
}
