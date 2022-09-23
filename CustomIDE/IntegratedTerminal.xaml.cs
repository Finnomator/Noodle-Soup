using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomIDE {
    public partial class IntegratedTerminal : UserControl {

        public IntegratedTerminal() {
            InitializeComponent();
        }

        private void MainTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Return:
                    break;
                case Key.Up:
                    e.Handled = true;
                    break;
                case Key.Down:
                    e.Handled = true;
                    break;
                case Key.Back:
                    break;
            }
        }
    }
}
