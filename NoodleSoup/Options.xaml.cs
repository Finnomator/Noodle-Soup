using NoodleSoup.Properties;
using Styles;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;

namespace NoodleSoup {
    public partial class Options : Window {
        private string[] AvailablePorts;
        public Options() {
            InitializeComponent();
            AvailablePorts = GetCOMPorts();
            SetContentsToSettings();
        }

        private void SetContentsToSettings() {
            AmpyStatus.Content = "Ampy: " + (Settings.Default.AmpyInstalled ? "Installed" : "Not installed");
            PythonStatus.Content = "Python: " + (Settings.Default.PythonInstalled ? "Installed" : "Not installed");
            InstallAmpyBut.IsEnabled = !Settings.Default.AmpyInstalled;
            InstallPythonBut.IsEnabled = !Settings.Default.PythonInstalled;

            string[] com_ports = AvailablePorts;
            for (int i = 0; i < com_ports.Length; i++) {
                string port = com_ports[i];
                ComPortItem comPortItem = new ComPortItem(port, Int32.Parse(port.Substring(3)));
                ComPortBox.Items.Add(comPortItem);
                if (Settings.Default.SelectedCOMPort == comPortItem.Port)
                    ComPortBox.SelectedIndex = i + 1;
            }
        }

        private void ApplyClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void InstallAmpy() {
            Process p = new Process {
                StartInfo = new ProcessStartInfo {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "pip",
                    Arguments = "install adafruit-ampy",
                }
            };

            p.Start();
            p.WaitForExit();
        }

        public string[] GetCOMPorts() {
            return SerialPort.GetPortNames();
        }

        public bool IsCOMPortAvailable(string com) {
            return GetCOMPorts().Contains(com);
        }

        public bool IsCOMPortAvailable(int com) {
            return IsCOMPortAvailable("COM" + com);
        }

        public bool IsAmpyInstalled() {
            Process p = new Process {
                StartInfo = new ProcessStartInfo {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "pip",
                    Arguments = "list",
                }
            };
            p.Start();

            return p.StandardOutput.ReadToEnd().Contains("adafruit-ampy ");
        }

        public bool IsPythonInstalled() {
            Process p = new Process {
                StartInfo = new ProcessStartInfo {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "python",
                    Arguments = "--version",
                }
            };
            p.Start();

            return p.StandardOutput.ReadToEnd().Contains("Python ");
        }

        private void InstallAmpyClick(object sender, RoutedEventArgs e) {
            AmpyStatus.Content = "Adafruit Ampy: Installing...";
            InstallAmpy();
            if (IsAmpyInstalled()) {
                AmpyStatus.Content = "Adafruit Ampy: Installed";
                Settings.Default.AmpyInstalled = true;
            } else {
                MessageBox.Show("Failed to install Ampy", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InstallPythonClick(object sender, RoutedEventArgs e) {
            Process.Start("https://www.python.org/downloads/");
        }

        private void UpdatePortBox() {
            ComPortBox.Items.Clear();
            ComPortBox.Items.Add(new ComPortItem("None", -1));
            foreach (string port in AvailablePorts) {
                ComPortBox.Items.Add(new ComPortItem(port, Int32.Parse(port.Substring(3))));
            }
        }


        private void OpenPortBox(object sender, System.EventArgs e) {
            UpdatePortBox();
        }

        private void ClosePortBox(object sender, System.EventArgs e) {
            if (ComPortBox.SelectedItem == null)
                ComPortBox.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Settings.Default.SelectedCOMPort = ((ComPortItem) ComPortBox.SelectedItem).Port;
            Settings.Default.Save();
        }

        private void RefreshClick(object sender, RoutedEventArgs e) {
            AvailablePorts = GetCOMPorts();
        }
    }

}
