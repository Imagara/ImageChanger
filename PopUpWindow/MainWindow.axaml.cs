using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace PopUpWindow
{
    public partial class MainWindow : Window
    {
        private readonly List<string> _pictures = new();
        private readonly int _screenNum = 1;
        private readonly Settings _settings = new();
        
        private readonly string _filePath;
        private readonly bool _autoDel;

        public MainWindow() => InitializeComponent();

        public MainWindow(int screenNum, string filePath = "", bool autoDel = true)
        {
            InitializeComponent();
            _screenNum = screenNum;
            _filePath = filePath;
            _autoDel = autoDel;
            Title = $"Screen #{screenNum}";
            ImportSettings();

            switch (MainSettings.Mode)
            {
                case 1:
                    if (filePath == "")
                    {
                        GetAllPictures();
                        MainImage.Source = _pictures.Count > 0
                            ? new Bitmap(_pictures.LastOrDefault())
                            : null;
                    }
                    else
                        MainImage.Source = new Bitmap(filePath);

                    break;
                case 2:
                    SecondModeCycle();
                    HelpGrid.IsVisible =
                        bool.TryParse(
                            new FileManager(Environment.CurrentDirectory).GetPrivateString("main", "leftpanel"),
                            out var leftPanel)
                            ? leftPanel
                            : false;
                    break;
                default:
                    new InfoWindow($"Invalid operating mode selected").Show();
                    break;
            }
        }

        private void ImportSettings()
        {
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
                _settings.Extensions = manager.GetPrivateString($"display{_screenNum}", "ext") != string.Empty
                    ? manager.GetPrivateString($"display{_screenNum}", "ext").Split('/')
                    : _settings.Extensions;
            }
            catch (Exception ex)
            {
                new InfoWindow(ex.Message).Show();
            }
        }

        private void ExportSettings()
        {
        }

        private async void SecondModeCycle()
        {
            GetAllPictures();
            while (true)
            {
                if (_pictures.Count == 0)
                    break;
                foreach (var item in _pictures)
                {
                    MainImage.Source = new Bitmap(item);
                    await Task.Delay(_settings.Rate * 1000);
                }
            }
        }

        private void GetAllPictures()
        {
            _pictures.Clear();

            foreach (string file in Directory
                         .EnumerateFiles(_settings.DirectoryPath, "*.*", SearchOption.AllDirectories)
                         .Where(item => _settings.Extensions.Any(ext => '.' + ext == Path.GetExtension(item))))
                _pictures.Add(file);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && MainSettings.Mode == 1)
            {
                foreach (Window window in MainSettings.Windows)
                    window.Close();
                MainSettings.Windows.Clear();
                
                FileInfo file = new FileInfo(_filePath);
                new FileManager(Environment.CurrentDirectory + "\\history.hy").WriteHistoryString(file.Name, file.LastWriteTime);
                
                if (_autoDel && _filePath != "" && file.Exists)
                    file.Delete();
            }
        }
    }
}