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
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Platform;

namespace PopUpWindow
{
    public class StartUp : Window
    {
        private readonly DateTime _targetTime;

        public StartUp()
        {
            //Create history.hy
            FileInfo historyFile = new FileInfo(Environment.CurrentDirectory + "\\history.hy");
            if (!historyFile.Exists)
                historyFile.Create();
            
            //Import main(general) settings
            ImportMainSettings();

            //Screen definition
            MainSettings.AllScreens = Screens.All;

            int mode = MainSettings.Mode;

            if (mode == 1)
            {
                string path = MainSettings.Directory + "\\StartUp.ini";

                if (new FileInfo(path).Exists) // if StartUp exists
                {
                    FileManager manager = new FileManager(path);
                    string fileName = manager.GetPrivateString("file");

                    if (fileName != "")
                    {
                        FileInfo file = new FileInfo(MainSettings.Directory + "\\" + fileName);
                        if (file.Exists && new FileManager(Environment.CurrentDirectory + "\\history.hy").IsHistoryContains(file.Name, file.LastWriteTime))
                            return;
                    }
                    else
                    {
                        //check other files (Is it possible to open and history not contains)
                        //if(!contains)
                        //
                    }

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
                        StartUpWaiter(); // need to post file path param
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
                    FileManager manager = new FileManager(MainSettings.Directory + "\\StartUp.ini");
                    string fileName = manager.GetPrivateString("file");
                    bool autoDel = bool.TryParse(manager.GetPrivateString("autodelete"), out var temp)
                        ? temp
                        : true;
                    if (fileName != "" && new FileInfo(MainSettings.Directory + "\\" + fileName).Exists)
                        OpenWindows(fileName, autoDel);
                    else
                        OpenWindows();
                    break;
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
            }
            catch (Exception ex)
            {
                new InfoWindow("Import main settings error: " + ex.Message).Show();
            }
        }

        private void OpenWindows(string filePath = "", bool autoDel = true)
        {
            int index = 1;
            foreach (var item in MainSettings.AllScreens)
            {
                var xPos = item.Bounds.Position.X;
                if (MainSettings.ScreensInUse.Any(x => x == index))
                {
                    MainWindow mw;
                    if (filePath != "")
                        mw = new MainWindow(index, filePath, autoDel);
                    else
                        mw = new MainWindow(index);
                    mw.Position = new Avalonia.PixelPoint(xPos, 0);
                    MainSettings.Windows.Add(mw);
                    mw.Show();
                }

                index++;
            }
        }
    }
}