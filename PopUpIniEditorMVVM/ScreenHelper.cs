using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input.Platform;
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