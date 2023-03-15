using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Platform;

namespace PopUpWindow
{
    public static class MainSettings
    {
        public static IReadOnlyList<Screen>? AllScreens { get; set; }
        public static byte[] ScreensInUse { get; set; } = { 1 };
        public static List<Window> Windows { get; } = new();
        public static string IniPath { get; } = Environment.CurrentDirectory + "\\settings.ini";
        public static int Mode { get; set; } = 1;
        public static string Directory { get; set; } = Environment.CurrentDirectory;
        public static int IniReaderRefreshRate { get; set; } = 1;
        public static bool AutoDeleteUsedFiles { get; set; } = true;
    }
}