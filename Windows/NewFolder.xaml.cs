using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace RaspCommander
{
    public partial class NewFolder : Window
    {
        public string Text { get; set; } = string.Empty;

        public NewFolder() => InitializeComponent();

        private void Button_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object sender, CancelEventArgs e) => DialogResult = !string.IsNullOrEmpty(Text = Text?.Trim());

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Click(sender, e);
                    break;
                case Key.Escape:
                    Text = null;
                    Button_Click(sender, e);
                    break;
            }
        }
    }
}
