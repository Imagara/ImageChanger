using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Drawing.Text;
using Avalonia.Platform;

namespace ImageChanger
{
    partial class StartUp : Window
    {
        public StartUp()
        {
            
        }
        public void ScreensDefinition()
        {
            MainSettings.AllScreens = Screens.All;
        }
        public void ShowWindow() //Test
        {
            MainWindow mw = new();
 
            mw.Position = new Avalonia.PixelPoint(1920,0);
            mw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mw.Show();
        }
    }
}
