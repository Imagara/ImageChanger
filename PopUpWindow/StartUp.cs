using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Avalonia;

namespace PopUpWindow
{
    public class StartUp : Window
    {
        private DateTime _targetTime;
        private readonly Logger _logger = new();

        public StartUp()
        {
            SlashDefinition();
            
            //Import main(general) settings
            ImportMainSettings();
            
            //Create history.hy if not exists
            string historyPath = Environment.CurrentDirectory + MainSettings.Slash + "history.hy";
            FileInfo historyFile = new FileInfo(historyPath);
            if (!historyFile.Exists)
                historyFile.Create();

            //Screens definition
            MainSettings.AllScreens = Screens.All;

            int mode = MainSettings.Mode;
            _logger.CreateLog($"{mode} mode selected");
            if (mode == 1)
                StartUpWaiter();
            else if (mode == 2)
                OpenWindows();
        }

        private void SlashDefinition()
        {
            if (Environment.OSVersion.ToString().Substring(0, 9) == "Microsoft")
                MainSettings.Slash = '\\';
            else if (Environment.OSVersion.ToString().Substring(0, 4) == "Unix")
                MainSettings.Slash = '/';
        }

        async void StartUpWaiter(int updateRate = 2)
        {
            string launchPath = MainSettings.Directory + MainSettings.Slash + "launch.ini";

            int seconds = updateRate;
            while (true)
            {
                if (new FileInfo(launchPath).Exists)
                {
                    FileManager manager = new FileManager(launchPath);

                    string dateTimeStr = manager.GetPrivateString("time");

                    Regex timeFormat = new Regex(@"^([0-1][0-9]|[2][1-3]):[0-5][0-9]");

                    DateTime targetTime = new();

                    if (timeFormat.IsMatch(dateTimeStr))
                        targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            Int32.Parse(dateTimeStr.Substring(0, dateTimeStr.IndexOf(':'))),
                            Int32.Parse(dateTimeStr.Substring(dateTimeStr.IndexOf(':') + 1,
                                dateTimeStr.Length - dateTimeStr.IndexOf(":", StringComparison.Ordinal) - 1)), 0);
                    else if (DateTime.TryParse(dateTimeStr, out DateTime tempdt))
                        targetTime = tempdt;

                    if (targetTime.Equals(DateTime.MinValue))
                        continue;
                    if (_targetTime != targetTime)
                        _logger.CreateLog($"StartUpWaiter: Next Start: {targetTime}");
                    _targetTime = targetTime;
                }

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
                            string historyPath = Environment.CurrentDirectory + MainSettings.Slash + "history.hy";
                            FileManager manager = new(historyPath);
                            if (!manager.IsHistoryContains(file.Name, file.LastWriteTime))
                                imagesPaths.Add(file.Name);
                        }

                        if (imagesPaths.Count > 0)
                        {
                            FileManager manager = new FileManager(launchPath);

                            bool autoDel = bool.TryParse(manager.GetPrivateString("autodelete"), out var temp)
                                ? temp
                                : true;
                            if (MainSettings.Windows.Count == 0)
                                OpenWindows(imagesPaths, autoDel);
                            _logger.CreateLog($"StartUpWaiter: Windows opened");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.CreateLog("An error occurred while running StartUpWaiter: " + ex.Message);
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

                _logger.CreateLog($"Main settings successfully imported");
            }
            catch (Exception ex)
            {
                _logger.CreateLog("Import main settings error: " + ex.Message);
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
                    if (filePaths != null && MainSettings.Mode == 1 && filePaths.Count > 0)
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