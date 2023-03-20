using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PopUpWindow;

public partial class IniCreatorWindow : Window
{
    public IniCreatorWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}