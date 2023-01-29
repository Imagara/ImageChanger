using Avalonia.Controls;
using Avalonia.Interactivity;
using static System.Net.Mime.MediaTypeNames;

namespace ImageChanger
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.Height;
        }
        public InfoWindow(string str)
        {
            InitializeComponent();
            InfoTB.Text = str;
            this.SizeToContent = SizeToContent.Height;
        }
        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
