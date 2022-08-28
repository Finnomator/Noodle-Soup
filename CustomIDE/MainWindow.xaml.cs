using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Linq;

namespace CustomIDE {

    public partial class MainWindow : Window {

        string current_file_path;
        Process CodeRunner;
        Options options;

        public MainWindow() {
            InitializeComponent();
            current_file_path = "Temp.py";
            options = new Options();

            options.UpdateSettings("Python", options.CheckPythonInstallation());
            options.UpdateSettings("Ampy", options.CeckAmpyInstallation());

            SetCodeBoxText(File.ReadAllText(current_file_path));        
        }

        private void OpenFileClick(object sender, RoutedEventArgs e) {
            if (SelectFile() == null)
                return;

            SetCodeBoxText(File.ReadAllText(current_file_path));
        }

        private string SelectFile() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if ((bool) !openFileDialog.ShowDialog())
                return null;
            current_file_path = openFileDialog.FileName;
            return current_file_path;
        }

        private void OpenDirClick(object sender, RoutedEventArgs e) {

        }

        private void SaveFile() {
            File.WriteAllText(current_file_path, GetCodeBoxText());
        }

        private void SaveClick(object sender, RoutedEventArgs e) {
            SaveFile();
        }

        private string GetCodeBoxText() {
            return new TextRange(CodeBox.Document.ContentStart, CodeBox.Document.ContentEnd).Text;
        }

        private void SetCodeBoxText(string text) {
            new TextRange(CodeBox.Document.ContentStart, CodeBox.Document.ContentEnd).Text = text;
        }

        private void SaveAsClick(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() != true)
                return;

            current_file_path = saveFileDialog.FileName;
            File.WriteAllText(current_file_path, GetCodeBoxText());
        }

        private void RunTerminalCommand(string fileName, string args) {
            OutputBox.Text = "Running " + fileName + " " + args + " ...\n[Output]\n\n";

            CodeRunner = new Process();
            CodeRunner.StartInfo.RedirectStandardError = true;
            CodeRunner.StartInfo.RedirectStandardOutput = true;
            CodeRunner.StartInfo.UseShellExecute = false;
            CodeRunner.StartInfo.CreateNoWindow = true;
            CodeRunner.StartInfo.FileName = fileName;
            CodeRunner.StartInfo.Arguments = args;

            CodeRunner.OutputDataReceived += new DataReceivedEventHandler((s, e) => {
                Dispatcher.Invoke(() => {
                    if (e.Data == null)
                        OutputBox.Text += "\n\n[Done]";
                    else
                        OutputBox.Text += e.Data.ToString();
                });
            });
            CodeRunner.ErrorDataReceived += new DataReceivedEventHandler((s, e) => {
                Dispatcher.Invoke(() => {
                    if (e.Data != null)
                        OutputBox.Text += e.Data.ToString();
                });
            });

            CodeRunner.Start();
            CodeRunner.BeginOutputReadLine();
            CodeRunner.BeginErrorReadLine();
        }

        private void StopTerminalCommand() {
            CodeRunner.Kill();
        }

        private void RunScriptClick(object sender, RoutedEventArgs e) {
            SaveFile();
            options.CheckCOMPort();
            if (options.settings["COM"] == "None") {
                MessageBox.Show("Select a COM Port first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else if (options.settings["Python"] == "Not Installed") {
                MessageBox.Show("Install Python first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else if (options.settings["Ampy"] == "Not Installed") {
                MessageBox.Show("Install adafruit-ampy first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            RunTerminalCommand("ampy", "--port " + options.settings["COM"] + " run " + current_file_path);
        }

        private void StopScriptClick(object sender, RoutedEventArgs e) {
            StopTerminalCommand();
        }

        private void OptionsClick(object sender, RoutedEventArgs e) {
            options = new Options();
            options.Show();
        }

        private void WindowCloses(object sender, EventArgs e) {
            SaveFile();
            options.Close();
            Application.Current.Shutdown();
        }

        private void CodeBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
           if (e.Key == Key.Return) {
                int lines = GetCodeBoxText().Split('\n').Length;
                if (lines == 1)
                    lines = 2;

                LineNumsBox.Text = "1\n";
                for(int i = 2; i <= lines; i++)
                    LineNumsBox.Text += i + "\n";
            }

            if (e.Key == Key.Back) {
                int lines = GetCodeBoxText().Split('\n').Length - 2;

                LineNumsBox.Text = "1\n";
                for (int i = 2; i <= lines; i++)
                    LineNumsBox.Text += i + "\n";
            }
        }
    }
}
