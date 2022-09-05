using CustomIDE;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Styles {
    public class CodyButton : Button {
        public CodyButton() : base() { }
    }

    public class FileButton : DirectoryButton {
        public FileButton() : base() { }
    }

    public class DirectoryButton : Button {

        public static Brush BackgroundColor;
        public static Brush SelectedColor;

        public DirectoryButton() : base() {

            Margin = new Thickness(0, 0, 0, 0);
            BorderThickness = new Thickness(0, 0, 0, 0);
            Padding = new Thickness(0, 0, 0, 0);
            FontSize = 10;
            FontFamily = new FontFamily("Cascadia Mono");

            HorizontalContentAlignment = HorizontalAlignment.Left;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            BorderThickness = new Thickness(0, 0, 0, 0);
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));

            BackgroundColor = Background;
            SelectedColor = new SolidColorBrush(Color.FromRgb(150, 150, 150));
        }
    }
}