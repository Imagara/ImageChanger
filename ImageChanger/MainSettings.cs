using System.Collections.Generic;
using Avalonia.Platform;

namespace ImageChanger
{
    public static class MainSettings
    {
        public static string? OS { get; set; }
        public static byte[]? ScreensInUse { get; set; }
        public static IReadOnlyList<Screen>? AllScreens { get; set; }
        public static string? INIPath { get; set; }
    }
}
