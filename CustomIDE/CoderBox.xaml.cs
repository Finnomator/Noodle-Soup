using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CustomIDE {
    public partial class CoderBox : UserControl {
        int lines_count = 1;

        public CoderBox() {
            InitializeComponent();
        }

        public string GetText() {
            return new TextRange(CodeTextBox.Document.ContentStart, CodeTextBox.Document.ContentEnd).Text;
        }

        public void SetText(string text) {
            new TextRange(CodeTextBox.Document.ContentStart, CodeTextBox.Document.ContentEnd).Text = text;
        }

        private void CodeBoxTextChange(object sender, System.Windows.Controls.TextChangedEventArgs e) {

            if (LineNumsBox == null)
                return;

            int line_breaks = GetText().Split('\n').Length - 1;

            if (lines_count == line_breaks)
                return;

            lines_count = line_breaks;

            LineNumsBox.Text = "1\n";
            for (int i = 2; i <= lines_count; i++) {
                LineNumsBox.Text += i + "\n";
            }
        }
    }
}
