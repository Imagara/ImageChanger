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
        private readonly List<string> _imagesPaths = new();
        private readonly int _screenNum = 1;

        private readonly Settings _settings = new();
        private readonly Logger _logger = new();

        private readonly bool _autoDel;

        public MainWindow() => InitializeComponent();

        public MainWindow(int screenNum, List<string> imagesPaths, bool autoDel = true)
        {
            InitializeComponent();
            try
            {
                _screenNum = screenNum;
                _imagesPaths = imagesPaths;
                _autoDel = autoDel;
                Title = $"Screen #{screenNum}";
                if (MainSettings.Mode == 1)
                    MainImage.Source = new Bitmap(MainSettings.Directory + MainSettings.Slash + imagesPaths.First());
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
                    FileManager manager = new FileManager(MainSettings.IniPath);
                    _settings.Rate = Byte.TryParse(manager.GetPrivateString($"display{_screenNum}", "rate"), out var temp)
                        ? temp
                        : _settings.Rate;
                    _settings.DirectoryPath =
                        manager.GetPrivateString($"display{_screenNum}", "directory").Trim() != string.Empty
                            ? manager.GetPrivateString($"display{_screenNum}", "directory")
                            : _settings.DirectoryPath;
                    HelpGrid.IsVisible =
                        bool.TryParse(
                            new FileManager(Environment.CurrentDirectory).GetPrivateString("main", "leftpanel"),
                            out var isVisible)
                            ? isVisible
                            : false;
                    
                    _logger.CreateLog($"{_screenNum} display: settings successfully imported");
                }
                catch (Exception ex)
                {
                    _logger.CreateLog($"{_screenNum} display: import settings error: " + ex.Message);
                }
                SecondModeCycle();
            }
            else
                Close();
        }

        private async void SecondModeCycle()
        {
            GetAllPictures();
            while (true)
            {
                if (_imagesPaths.Count == 0)
                    break;
                foreach (var item in _imagesPaths)
                {
                    MainImage.Source = new Bitmap(item);
                    await Task.Delay(_settings.Rate * 1000);
                }
            }
        }

        private void GetAllPictures()
        {
            _imagesPaths.Clear();

            foreach (string file in Directory
                         .EnumerateFiles(_settings.DirectoryPath, "*.*", SearchOption.TopDirectoryOnly)
                         .Where(item => MainSettings.Extensions.Any(ext => '.' + ext == Path.GetExtension(item))))
            {
                _imagesPaths.Add(file);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && MainSettings.Mode == 1)
            {
                FileInfo file = new FileInfo(MainSettings.Directory + MainSettings.Slash + _imagesPaths.First());
                string historyPath = Environment.CurrentDirectory + MainSettings.Slash + "history.hy";

                new FileManager(historyPath).WriteHistoryString(file.Name, file.LastWriteTime);
                if (_autoDel && file.Exists)
                    file.Delete();
                _imagesPaths.Remove(_imagesPaths.First());
                if (_imagesPaths.Count <= 0)
                {
                    foreach (Window window in MainSettings.Windows)
                        window.Close();
                    MainSettings.Windows.Clear();
                }
                else
                    MainImage.Source = new Bitmap(MainSettings.Directory + MainSettings.Slash + _imagesPaths.First());
            }
        }
    }
}