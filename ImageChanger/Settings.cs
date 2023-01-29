using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageChanger
{
    public static class Settings
    {
        public static string OS = "Windows";
        // Windows // Linux
        public static int Mode = 1;
        public static string PicturesDirectoryPath = Environment.CurrentDirectory;
        public static int Rate = 5;
    }
}
