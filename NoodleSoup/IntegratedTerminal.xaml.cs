using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace NoodleSoup {
    public partial class IntegratedTerminal : UserControl {

        public delegate void CmdFinishedHandler(object sender, EventArgs e);
        public event CmdFinishedHandler OnCmdFinished;
        private Cmd cmd;
        public string WorkingDirectory { get; private set; }

        public IntegratedTerminal() {
            InitializeComponent();
            WorkingDirectory = "";

            cmd = new Cmd("");

            cmd.OutputDataReceived += Cmd_OutputDataReceived;
            cmd.ErrorDataReceived += Cmd_ErrorDataReceived;
        }

        private int GetCaretCol() => MainTextBox.CaretIndex - MainTextBox.GetCharacterIndexFromLineIndex(GetCaretRow());

        private int GetCaretRow() => MainTextBox.GetLineIndexFromCharacterIndex(MainTextBox.CaretIndex);

        private void Cmd_Exited(object sender, EventArgs e) {
            OnCmdFinished(cmd, e);
        }

        private void Cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            Dispatcher.Invoke((Action) delegate () {
                if (e.Data != null)
                    AppendText(e.Data + "\n");
            });
        }

        private void Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Dispatcher.Invoke((Action) delegate () {
                if (e.Data != null)
                    AppendText(e.Data + "\n");
            });
        }

        private void MainTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Return:
                    string command = MainTextBox.GetLineText(GetCaretRow());
                    if (cmd.HasExited) {
                        Run(command);
                    } else {
                        cmd.StandardInput.WriteLine(command);
                    }
                    break;
                case Key.Up:
                    e.Handled = true;
                    break;
                case Key.Down:
                    e.Handled = true;
                    break;
                case Key.Back:
                    if (GetCaretCol() > 0) {
                        RemoveText(-1);
                    }
                    break;
                case Key.Left:
                    if (GetCaretCol() == 0) {
                        e.Handled = true;
                    }
                    break;
                case Key.Right:
                    if (MainTextBox.CaretIndex == MainTextBox.Text.Length)
                        e.Handled = true;
                    break;
                case Key.Delete:
                    if (MainTextBox.CaretIndex < MainTextBox.Text.Length) {
                        RemoveText(1);
                    }
                    break;
                case Key.Space:
                    AppendText(" ");
                    break;
            }
        }

        private void MainTextBox_TextInput(object sender, TextCompositionEventArgs e) {

            if (e.Text == "\n" || e.Text == "\r")
                return;

            AppendText(e.Text);
        }

        private void CutOutSequence(int start, int end) {
            if (end > start)
                MainTextBox.Text = MainTextBox.Text.Substring(0, start) + MainTextBox.Text.Substring(end);
            else if (end < start)
                MainTextBox.Text = MainTextBox.Text.Substring(0, end) + MainTextBox.Text.Substring(start);
        }

        private void RemoveText(int length) {
            int old_idx = MainTextBox.CaretIndex;
            CutOutSequence(old_idx, old_idx + length);

            if (length < 0)
                MainTextBox.CaretIndex = old_idx - length;
            else
                MainTextBox.CaretIndex = old_idx;
        }

        private void AppendText(string text) {
            int old_idx = MainTextBox.CaretIndex;
            MainTextBox.Text += text;
            MainTextBox.CaretIndex = old_idx + text.Length;
        }

        public void Run(string command) {
            cmd = new Cmd(command);
        }

        public void Stop() {
            cmd.Kill();
        }

        public void ChangeDir(string working_dir) {
            AppendText($"{working_dir}> ");
            WorkingDirectory = working_dir;
        }

        private void MainTextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (GetCaretCol() < WorkingDirectory.Length + 2)
                MainTextBox.CaretIndex = GetCaretRow() + WorkingDirectory.Length + 2;
        }
    }

    public class Cmd : Process {

        public Cmd(string command) : base() {

            EnableRaisingEvents = true;

            StartInfo = new ProcessStartInfo {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                FileName = "cmd.exe",
                Arguments = "/c " + command,
            };

            Start();

            BeginOutputReadLine();
            BeginErrorReadLine();
        }
    }
}
