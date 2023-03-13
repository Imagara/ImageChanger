using System;
using System.Collections.Generic;
using Avalonia.Platform;

namespace PopUpWindow
{
    public static class MainSettings
    {
        public const char Slash = '\\';
        public static IReadOnlyList<Screen>? AllScreens { get; set; }
        public static string IniPath { get;} = Environment.CurrentDirectory + Slash + "settings.ini";
        public static byte[] ScreensInUse { get; set; } = { 1 };
        public static int Mode { get; set; } = 1;
        public static string Directory { get; set; } = Environment.CurrentDirectory;
        public static int IniReaderRefreshRate { get; set; } = 1;
    }
}