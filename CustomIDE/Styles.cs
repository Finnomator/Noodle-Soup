using CustomIDE;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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

    public class TabItemButton : Grid {

        public Button selectButton;
        public Button closeButton;

        public TabItemButton(string title) : base() {

            HorizontalAlignment = HorizontalAlignment.Left;

            ColumnDefinitions.Add(new ColumnDefinition {
                Width = GridLength.Auto,
            });
            ColumnDefinitions.Add(new ColumnDefinition {
                Width = GridLength.Auto
            });

            selectButton = new Button {
                Content = title,
                Background = null,
                Tag = this,
                Foreground = Brushes.White,
                Padding = new Thickness(1, 0, 3, 0),
                BorderThickness = new Thickness(0, 0, 0, 0),
                FontSize = 10,
            };

            closeButton = new Button {
                Content = "X",
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0, 0, 0, 0),
                FontSize = 10,
                Background = null,
                Tag = this,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            selectButton.MouseEnter += SelectButton_MouseEnter;
            selectButton.MouseLeave += SelectButton_MouseLeave;

            selectButton.SetValue(Grid.ColumnProperty, 0);
            closeButton.SetValue(Grid.ColumnProperty, 1);

            Children.Add(closeButton);
            Children.Add(selectButton);

        }

        private void SelectButton_MouseLeave(object sender, MouseEventArgs e) {
        }

        private void SelectButton_MouseEnter(object sender, MouseEventArgs e) {
        }
    }
}