using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Platform;

namespace PopUpIniEditorMVVM;

public class ScreenHelper : Window
{
    public static IReadOnlyList<Screen> AllScreens;

    public ScreenHelper()
    {
        AllScreens = Screens.All;
    }
}