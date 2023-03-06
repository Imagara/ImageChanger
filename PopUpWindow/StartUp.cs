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

namespace PopUpWindow
{
    class StartUp : Window
    {
        public StartUp()
        {
            OSDefinition();
            INIPathDefinition();
            ImportMainSettings();
            ScreensDefinition();
            OpenWindows();
        }

        public void ScreensDefinition()
        {
            MainSettings.AllScreens = Screens.All;
        }

        private void ImportMainSettings()
        {
            try
            {
                INIManager manager = new INIManager(MainSettings.INIPath);
                MainSettings.ScreensInUse = manager.GetPrivateString($"main", "screens") != string.Empty
                    ? manager.GetPrivateString($"main", "screens").Split('/')
                        .Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => byte.Parse(i)).ToArray()
                    : new byte[] { 1 };
                //MainSettings.RefreshRate = manager.GetPrivateString("main","refreshrate")
            }
            catch (Exception ex)
            {
                new InfoWindow("Import main settings error: " + ex.Message).Show();
            }
        }

        private void OpenWindows()
        {
            int xPos = 0, index = 1;
            foreach (var item in MainSettings.AllScreens)
            {
                xPos = item.Bounds.Position.X;
                if (MainSettings.ScreensInUse.Any(x => x == index))
                    ShowWindow(xPos, index);
                index++;
            }
        }

        private void ShowWindow(int xPos, int screenNum)
        {
            new MainWindow(screenNum)
            {
                Position = new Avalonia.PixelPoint(xPos, 0),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            }.Show();
        }

        private void OSDefinition()
        {
            if (Environment.OSVersion.ToString().Substring(0, 9) == "Microsoft")
                MainSettings.OS = "Windows";
            else if (Environment.OSVersion.ToString().Substring(0, 4) == "Unix")
                MainSettings.OS = "Unix";
        }

        private void INIPathDefinition()
        {
            if (MainSettings.OS == "Windows")
                MainSettings.INIPath = Environment.CurrentDirectory + "\\settings.ini";
            else if (MainSettings.OS == "Unix")
                MainSettings.INIPath = Environment.CurrentDirectory + "/settings.ini";
            else
                new InfoWindow("Ошибка определения пути к ini файлу.").Show();
        }
    }
}