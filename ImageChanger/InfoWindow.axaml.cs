using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ImageChanger
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }
        public InfoWindow(string str)
        {
            InitializeComponent();
            InfoTB.Text = str;
            this.SizeToContent = SizeToContent.Height;
            if (str.Length < 150)
                this.Width = str.Length + 150;

        }
        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
