using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace ImageChanger
{
    public partial class MainWindow : Window
    {
        private List<string> pictures = new List<string>();
        private int screenNum = 1;
        public Settings settings = new();

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        public MainWindow(int _screenNum)
        {
            InitializeComponent();
            screenNum = _screenNum;
            Start();
        }
        private void Start()
        {
            this.Title = $"Screen #{screenNum}";
            ImportSettings(); 
            GetAllPictures();
            switch (settings.Mode)
            {
                case 1:
                    MainImage.Source = pictures.Count > 0 ? new Bitmap(pictures.LastOrDefault()) : null;
                    break;
                case 2:
                    Dispatcher.UIThread.Post(action: () => SecondModeCycle(), priority: DispatcherPriority.Background);
                    break;
                default:
                    new InfoWindow($"Режим не найден. ({settings.Mode})").Show();
                    break;
            }
        }
        private void ImportSettings()
        {
            try
            {
                INIManager manager = new INIManager(MainSettings.INIPath);

                //Импорт настроек из ini файла.
                Byte temp;
                settings.Mode = Byte.TryParse(manager.GetPrivateString($"display{screenNum}", "mode"), out temp) ? temp : settings.Mode;
                settings.Rate = Byte.TryParse(manager.GetPrivateString($"display{screenNum}", "rate"), out temp) ? temp : settings.Rate;
                settings.PicturesDirectoryPath = manager.GetPrivateString($"display{screenNum}", "picdirectory").Trim() != string.Empty ? manager.GetPrivateString($"display{screenNum}", "picdirectory") : settings.PicturesDirectoryPath;
                settings.Extensions = manager.GetPrivateString($"display{screenNum}", "ext") != string.Empty ? manager.GetPrivateString($"display{screenNum}", "ext").Split('/') : settings.Extensions;
            }
            catch (Exception ex)
            {
                new InfoWindow(ex.Message).Show();
            }

        }
        private void ExportSettings()
        {

        }

        private async Task SecondModeCycle()
        {
            while (true)
            {
                if (pictures.Count == 0)
                    break;
                foreach (var item in pictures)
                {
                    MainImage.Source = new Bitmap(item);
                    await Task.Delay(settings.Rate * 1000);
                }
            }
        }
        private void GetAllPictures()
        {

            pictures.Clear();


            foreach (string file in Directory.EnumerateFiles(settings.PicturesDirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(item => settings.Extensions.Any(ext => '.' + ext == Path.GetExtension(item))))
                pictures.Add(file);
        }
        private void OnImportButtonClick(object sender, RoutedEventArgs e)
        {
            //ImportSettings();
            //GetAllPictures();
            //Dispatcher.UIThread.Post(() => SecondModeCycle(), DispatcherPriority.Background);
            new InfoWindow("Current settings:\n" +
                $"Screen:{screenNum}\n" +
                $"OS:{MainSettings.OS}\n" +
                $"Mode:{settings.Mode}\n" +
                $"Rate:{settings.Rate}sec\n" +
                $"PictureDirectory:{settings.PicturesDirectoryPath}\n" +
                $"FileExtensions:{string.Join("/", settings.Extensions)}" +
                $"\n\n{string.Join("\n", pictures)}").Show();
        }
    }
}
