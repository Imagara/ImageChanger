using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PopUpWindow
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()=>InitializeComponent();
        public InfoWindow(string str)
        {
            InitializeComponent();
            InfoTB.Text = str;
            SizeToContent = SizeToContent.Height;
            if (str.Length < 150)
                Width = str.Length + 200;
        }
        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
