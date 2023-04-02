using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PopUpIniEditorMVVM.Views;

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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}