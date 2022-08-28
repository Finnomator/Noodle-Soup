using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;

namespace CustomIDE {
    public partial class Options : Window {
        public Dictionary<string, string> settings;
        string[] ports;
        public Options() {
            InitializeComponent();

            settings = LoadSettings();

            ComPortBox.SelectedItem = CheckCOMPort();

            AmpyStatus.Content = "Adafruit Ampy: " + settings["Ampy"];
            PythonStatus.Content = "Python: " + settings["Python"];
            InstallAmpyBut.IsEnabled = settings["Ampy"] == "Not Installed";
            InstallPythonBut.IsEnabled = settings["Python"] == "Not Installed";
        }

        public Dictionary<string, string> LoadSettings() {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("settings.json"));
        }

        public void UpdateSettings(string option, string value) {
            settings[option] = value;
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
        }

        private void ApplyClick(object sender, RoutedEventArgs e) {
            UpdateSettings("COM", ComPortBox.SelectedItem.ToString());
            Close();
        }

        private void InstallAmpy() {
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "pip";
            p.StartInfo.Arguments = "install adafruit-ampy";
            p.Start();
            p.WaitForExit();
        }

        public string CeckAmpyInstallation() {
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "pip";
            p.StartInfo.Arguments = "list";
            p.Start();
            p.WaitForExit();

            return p.StandardOutput.ReadToEnd().Contains("adafruit-ampy ") ? "Installed" : "Not Installed";
        }

        public string CheckPythonInstallation() {
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "python";
            p.StartInfo.Arguments = "--version";
            p.Start();
            p.WaitForExit();

            return p.StandardOutput.ReadToEnd().Contains("Python ") ? "Installed" : "Not Installed";
        }

        private void InstallAmpyClick(object sender, RoutedEventArgs e) {
            AmpyStatus.Content = "Adafruit Ampy: Installing...";
            InstallAmpy();
            if (CeckAmpyInstallation() == "Installed") {
                UpdateSettings("Ampy", "Installed");
                AmpyStatus.Content = "Adafruit Ampy: Installed";
            } else {
                MessageBox.Show("Failed to install Ampy", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InstallPythonClick(object sender, RoutedEventArgs e) {
            Process.Start("https://www.python.org/downloads/");
        }

        public string CheckCOMPort() {
            ports = SerialPort.GetPortNames();
            if (ports.Contains(settings["COM"])) {
                return settings["COM"];
            } else {
                UpdateSettings("COM", "None");
                return "None";
            }
        }


        private void OpenPortBox(object sender, System.EventArgs e) {
            CheckCOMPort();
            ComPortBox.Items.Clear();
            ComPortBox.Items.Add("None");
            foreach (string port in ports) {
                ComPortBox.Items.Add(port);
            }
        }

        private void ClosePortBox(object sender, System.EventArgs e) {
            if (ComPortBox.SelectedItem == null)
                ComPortBox.SelectedItem = "None";
        }
    }
}
