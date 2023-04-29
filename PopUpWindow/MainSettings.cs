using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform;

namespace PopUpWindow
{
    public static class MainSettings
    {
        public static IReadOnlyList<Screen>? AllScreens { get; set; }
        public static byte[] ScreensInUse { get; set; } = { 1 };
        public static List<Window> Windows { get; } = new();
        public static string IniPath { get; } = Path.Combine(Environment.CurrentDirectory, "settings.ini");
        public static int Mode { get; set; } = 1;
        public static string Directory { get; set; } = Environment.CurrentDirectory;
        public static int IniReaderRefreshRate { get; set; } = 5;
        public static TimeOnly ActivityStart { get; set; } = new(9, 0);
        public static TimeOnly ActivityEnd{ get; set; } = new(21, 0);
        public static string[] Extensions { get; set; } = { ".png", ".jpeg", ".jpg", ".bmp", ".tiff", ".jfif", ".webp" };
    }
}