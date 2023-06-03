using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PopUpIniEditorMVVM.Views;

public partial class ScreenDefinitionHelpWindow : Window
{
    private int ScreenNum { get; set; } = 0;
    
    public ScreenDefinitionHelpWindow()=>InitializeComponent();

    public ScreenDefinitionHelpWindow(int screenNum = 0)
    {
        InitializeComponent();
        ScreenNum = screenNum;
        DataContext = this;
    }
}