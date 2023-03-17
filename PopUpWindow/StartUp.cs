using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Drawing.Text;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia;
using Avalonia.Platform;

namespace PopUpWindow
{
    public class StartUp : Window
    {
        private readonly DateTime _targetTime;

        public StartUp()
        {
            //Import main(general) settings
            ImportMainSettings();

            //Create history.hy if not exists
            string historyPath = Environment.CurrentDirectory + "\\history.hy";
            FileInfo historyFile = new FileInfo(historyPath);
            if (!historyFile.Exists)
                historyFile.Create();

            //Screens definition
            MainSettings.AllScreens = Screens.All;

            int mode = MainSettings.Mode;
            if (mode == 1)
            {
                string launchPath = MainSettings.Directory + "\\launch.ini";
                if (new FileInfo(launchPath).Exists)
                {
                    FileManager manager = new FileManager(launchPath);

                    string dateTimeStr = manager.GetPrivateString("time");

                    Regex timeFormat = new Regex(@"^([0-1][0-9]|[2][1-3]):[0-5][0-9]");

                    if (timeFormat.IsMatch(dateTimeStr))
                        _targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            Int32.Parse(dateTimeStr.Substring(0, dateTimeStr.IndexOf(':'))),
                            Int32.Parse(dateTimeStr.Substring(dateTimeStr.IndexOf(':') + 1,
                                dateTimeStr.Length - dateTimeStr.IndexOf("=", StringComparison.Ordinal) - 1)), 0);
                    else if (DateTime.TryParse(dateTimeStr, out DateTime tempdt))
                        _targetTime = tempdt;

                    if (!_targetTime.Equals(DateTime.MinValue))
                        StartUpWaiter();
                }
            }
            else if (mode == 2)
            {
                OpenWindows();
            }
        }

        async void StartUpWaiter()
        {
            new InfoWindow($"StartUpWaiter \nNext Start: {_targetTime}").Show();
            int seconds = 5;
            while (true)
            {
                if (DateTime.Now > _targetTime)
                {
                    try
                    {
                        List<string> imagesPaths = new();
                        string[] extensions = { "png", "jpeg", "jpg", "bmp", "tiff", "jfif", "webp" };

                        var dir = new DirectoryInfo(MainSettings.Directory);

                        FileInfo[] files = dir.GetFiles();

                        foreach (FileInfo file in files
                                     .Where(item => extensions.Any(ext => '.' + ext == item.Extension))
                                     .OrderBy(ord => ord.LastWriteTime))
                        {
                            string historyPath = Environment.CurrentDirectory + "\\history.hy";
                            FileManager manager = new(historyPath);
                            if (!manager.IsHistoryContains(file.Name, file.LastWriteTime))
                                imagesPaths.Add(file.Name);
                        }

                        if (imagesPaths.Count > 0)
                        {
                            string launchPath = MainSettings.Directory + "\\launch.ini";
                            FileManager manager = new FileManager(launchPath);

                            bool autoDel = bool.TryParse(manager.GetPrivateString("autodelete"), out var temp)
                                ? temp
                                : true;
                            OpenWindows(imagesPaths, autoDel);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        new InfoWindow(ex.Message).Show();
                        throw;
                    }
                }

                await Task.Delay(seconds * 1000);
            }
        }

        private void ImportMainSettings()
        {
            try
            {
                FileManager manager = new FileManager(MainSettings.IniPath);

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

                MainSettings.Directory =
                    manager.GetPrivateString($"main", "directory").Trim() != string.Empty
                        ? manager.GetPrivateString($"main", "directory")
                        : MainSettings.Directory;
            }
            catch (Exception ex)
            {
                new InfoWindow("Import main settings error: " + ex.Message).Show();
            }
        }

        private void OpenWindows(List<string> filePaths = null, bool autoDel = true)
        {
            int index = 1;
            foreach (var item in MainSettings.AllScreens)
            {
                var xPos = item.Bounds.Position.X;
                if (MainSettings.ScreensInUse.Any(x => x == index))
                {
                    MainWindow mw;
                    if (filePaths.Count > 0)
                        mw = new MainWindow(index, filePaths, autoDel);
                    else
                        mw = new MainWindow(index);
                    mw.Position = new PixelPoint(xPos, 0);
                    MainSettings.Windows.Add(mw);
                    mw.Show();
                }

                index++;
            }
        }
    }
}