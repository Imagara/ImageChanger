using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform;


namespace ImageChanger
{
    public static class MainSettings
    {
        public static string OS { get; set; }

        public static Screen[] ScreenInUse { get; set; }
        public static IReadOnlyList<Screen> AllScreens { get; set; }
    }
}
