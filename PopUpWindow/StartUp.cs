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
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Platform;

namespace PopUpWindow
{
    class StartUp : Window
    {
        private DateTime _targetTime;

        public StartUp()
        {
            //Import main(general) settings
            ImportMainSettings();

            //Screen definition
            MainSettings.AllScreens = Screens.All;

            int mode = MainSettings.Mode;

            if (mode == 1)
            {
                string path = MainSettings.Directory + MainSettings.Slash + "StartUp.ini";
                FileInfo file = new(path);

                if (file.Exists)
                {
                    IniManager manager = new(path);

                    string dateTimeStr = manager.GetPrivateString("main", "time");
                    Regex timeFormat = new Regex(@"^([0-1][0-9]|[2][1-3])[:./\s-][0-5][0-9]");

                    dateTimeStr = "00 00"; //temp
                    
                    if (timeFormat.IsMatch(dateTimeStr))
                        _targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            Int32.Parse(dateTimeStr.Substring(0, dateTimeStr.IndexOf(' '))),
                            Int32.Parse(dateTimeStr.Substring(3, 2)), 0);
                    else if (DateTime.TryParse(dateTimeStr, out DateTime tempdt))
                        _targetTime = tempdt;

                    if (!_targetTime.Equals(DateTime.MinValue))
                        StartUpWaiter();
                }
            }
            else if (mode == 2)
            {
                //OpenWindows();
            }
        }

        void StartUpWaiter()
        {
            int seconds = 5;
            while (true)
            {
                if (DateTime.Now > _targetTime)
                {
                    OpenWindows();
                    break;
                }
                Thread.Sleep(seconds * 1000);
            }
        }

        private void ImportMainSettings()
        {
            try
            {
                IniManager manager = new IniManager(MainSettings.IniPath);
                MainSettings.Mode = Int32.TryParse(manager.GetPrivateString("main", "mode"), out var mode)
                    ? mode
                    : MainSettings.Mode;
                MainSettings.ScreensInUse = manager.GetPrivateString($"main", "screens") != string.Empty
                    ? manager.GetPrivateString($"main", "screens").Split('/')
                        .Where(i => !string.IsNullOrWhiteSpace(i)).Select(byte.Parse).ToArray()
                    : new byte[] { 1 };
                MainSettings.IniReaderRefreshRate =
                    Byte.TryParse(manager.GetPrivateString("main", "inirefreshrate"), out var temp)
                        ? temp
                        : MainSettings.IniReaderRefreshRate;
            }
            catch (Exception ex)
            {
                new InfoWindow("Import main settings error: " + ex.Message).Show();
            }
        }

        private void OpenWindows()
        {
            int index = 1;
            foreach (var item in MainSettings.AllScreens)
            {
                var xPos = item.Bounds.Position.X;
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
    }
}