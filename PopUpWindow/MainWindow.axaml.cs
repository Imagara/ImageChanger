using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using Avalonia.Input;

namespace PopUpWindow
{
    public partial class MainWindow : Window
    {
        private List<string> _imagesPaths = new();
        private readonly int _screenNum = 1;
        private readonly Settings _settings = new();
        private readonly Logger _logger = new();

        private readonly bool _autoDel;

        public MainWindow() => InitializeComponent();

        public MainWindow(int screenNum, List<string> imagesPaths, bool autoDel = true)
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
            _screenNum = screenNum;
            _imagesPaths = imagesPaths;
            _autoDel = autoDel;

            Title = $"Screen #{screenNum}";

            try
            {
                if (MainSettings.Mode == 1)
                {
                    MainImage.Source = new Bitmap(Path.Combine(MainSettings.Directory, _imagesPaths.First()));
                }
                else
                    Close();
            }
            catch (Exception ex)
            {
                _logger.CreateLog("Exception when starting the first mode: " + ex.Message);
            }
        }

        public MainWindow(int screenNum)
        {
            InitializeComponent();
            _screenNum = screenNum;
            Title = $"Screen #{screenNum}";
            if (MainSettings.Mode == 2)
            {
                //Import window settings
                try
                {
                    var manager = new FileManager(MainSettings.IniPath);

                    if (byte.TryParse(manager.GetPrivateString($"display{_screenNum}", "rate"), out var temp))
                    {
                        _settings.Rate = temp;
                    }

                    if (string.IsNullOrWhiteSpace(manager.GetPrivateString($"display{_screenNum}", "directory")
                            .Trim()) == false)
                    {
                        _settings.DirectoryPath = manager.GetPrivateString($"display{_screenNum}", "directory");
                    }

                    if (bool.TryParse(manager.GetPrivateString("main", "leftpanel"),
                            out var isVisible))
                    {
                        HelpGrid.IsVisible = isVisible;
                    }

                    _logger.CreateLog($"{_screenNum} display: settings successfully imported");
                }
                catch (Exception ex)
                {
                    _logger.CreateLog($"{_screenNum} display: import settings error: {ex.Message}");
                }

                SecondModeCycle();
            }
            else
                Close();
        }
        void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var window in MainSettings.Windows.ToList())
            {
                window.Close();
                MainSettings.Windows.Remove(window);
            }
        }

        private async void SecondModeCycle()
        {
            while (true)
            {
                GetAllPictures();
                if (_imagesPaths.Count == 0)
                    await Task.Delay(1800 * 1000);;
                foreach (var item in _imagesPaths)
                {
                    MainImage.Source = new Bitmap(item);
                    await Task.Delay(_settings.Rate * 1000);
                }
            }
        }

        private void GetAllPictures()
        {
            try
            {
                _imagesPaths.Clear();
                
                _imagesPaths.AddRange(Directory
                    .EnumerateFiles(_settings.DirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(filePath => MainSettings.Extensions.Any(ext => ext.Equals(Path.GetExtension(filePath)))));

                _logger.CreateLog(
                    $"{_screenNum} display: got {_imagesPaths.Count} files from {_settings.DirectoryPath}");
            }
            catch (Exception ex)
            {
                _logger.CreateLog($"{_screenNum} display: error: {ex.Message}");
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && MainSettings.Mode == 1)
            {
                var file = new FileInfo(Path.Combine(MainSettings.Directory, _imagesPaths.First()));
                var historyPath = Path.Combine(Environment.CurrentDirectory, "history.hy");
                var fileManager = new FileManager(historyPath);
                try
                {
                    fileManager.WriteHistoryString(file.Name, file.LastWriteTime);

                    if (_autoDel && file.Exists)
                    {
                        file.Delete();
                    }

                    _imagesPaths.Remove(_imagesPaths.First());

                    if (_imagesPaths.Count <= 0)
                    {
                        foreach (var window in MainSettings.Windows.ToList())
                        {
                            window.Close();
                            MainSettings.Windows.Remove(window);
                        }
                    }
                    else
                    {
                        MainImage.Source =
                            new Bitmap(Path.Combine(MainSettings.Directory, _imagesPaths.First()));
                    }
                }
                catch (Exception ex)
                {
                    _logger.CreateLog($"{_screenNum} display: error: {ex.Message}");
                }
            }
        }
    }
}