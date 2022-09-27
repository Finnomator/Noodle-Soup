using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace NoodleSoup {
    public partial class IntegratedTerminal : UserControl {

        public delegate void CmdFinishedHandler(object sender, EventArgs e);
        public event CmdFinishedHandler OnCmdFinished;
        private string OldInputText = "";

        public IntegratedTerminal() {
            InitializeComponent();
            PrintWorkingDir();
        }

        private void PrintWorkingDir() {
            OutputTextBlock.Text += Cmd.WorkingDirectory + ">";
        }

        private void Cmd_Exited(object sender, EventArgs e) {

            try {
                if (!Cmd.P.HasExited) {
                    Cmd.P.Kill();
                    Cmd.P.Close();
                }
            } catch (InvalidOperationException) {

            }

            Dispatcher.Invoke((Action) delegate () {
                PrintWorkingDir();
                OnCmdFinished(this, e);
            });
        }

        private void Cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            Dispatcher.Invoke((Action) delegate () {
                if (e.Data != null) {
                    OutputTextBlock.Text += e.Data + "\n";
                    OutputScroller.ScrollToBottom();
                }
            });
        }

        private void Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Dispatcher.Invoke((Action) delegate () {
                if (e.Data != null) {
                    OutputTextBlock.Text += e.Data + "\n";
                    OutputScroller.ScrollToBottom();
                } else
                    Cmd_Exited(this, new EventArgs());
            });
        }

        public void Run(string command) {

            command = command.Trim();

            OutputTextBlock.Text += command + "\n";

            if (command == "cls") {
                OutputTextBlock.Text = "";
                PrintWorkingDir();
                return;
            }

            Cmd.RunWithReadLine(command);
            Cmd.P.OutputDataReceived += Cmd_OutputDataReceived;
            Cmd.P.ErrorDataReceived += Cmd_ErrorDataReceived;
        }

        public void Stop() {
            Cmd_Exited(this, new EventArgs());
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Return:
                    e.Handled = true;

                    string command = InputTextBox.Text;

                    if (command == "")
                        break;

                    InputTextBox.Text = "";
                    if (Cmd.P.HasExited)
                        Run(command);
                    else
                        Cmd.P.StandardInput.WriteLine(command);
                    break;
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (OldInputText.Length > 0)
                OutputTextBlock.Text = OutputTextBlock.Text.Remove(OutputTextBlock.Text.Length - OldInputText.Length);
            OutputTextBlock.Text += InputTextBox.Text;
            OldInputText = InputTextBox.Text;
        }
    }

    public class Cmd {

        public static Process P { get; private set; }
        public static string WorkingDirectory { get; private set; }
        private static char Drive;

        static Cmd() {
            P = new Process();
            WorkingDirectory = @"C:\Users\" + Environment.UserName;
            Drive = 'c';
            RunAndGetOutput("");
        }

        private static void SetProcess(string command) {

            command = command.Trim();

            P = new Process {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = "cmd.exe",
                }
            };

            if (command.Length == 2 && command[1] == ':') {
                Drive = command[0];
                string dir = GetCDTarget(command);

                if (dir != "")
                    WorkingDirectory = dir;
            }

            if (command.StartsWith("cd ") && !command.Contains("/?")) {

                string dir = GetCDTarget(command);

                if (dir != "")
                    WorkingDirectory = dir;
            }

            P.StartInfo.Arguments = $"/c cd {WorkingDirectory} && {Drive}: && {command}";
        }

        public static void RunWithReadLine(string command) {

            SetProcess(command);

            P.Start();

            if (command.StartsWith("cd ") && !command.Contains("/?")) {
                P.Kill();
            }

            P.BeginOutputReadLine();
            P.BeginErrorReadLine();
        }

        public static Tuple<string, string> RunAndGetOutput(string command) {

            SetProcess(command);

            P.Start();

            if (command.StartsWith("cd ") && !command.Contains("/?")) {
                P.Kill();
                return Tuple.Create("", "");
            }

            return Tuple.Create(P.StandardOutput.ReadToEnd(), P.StandardError.ReadToEnd());
        }

        private static string GetCDTarget(string cd_command) {
            Process finder = new Process {
                StartInfo = new ProcessStartInfo {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName = "cmd.exe",
                    Arguments = $"/c {Drive}: && cd {WorkingDirectory} && {cd_command} && cd",
                }
            };

            finder.Start();
            return finder.StandardOutput.ReadToEnd().TrimEnd();
        }
    }
}
