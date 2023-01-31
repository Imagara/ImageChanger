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
        private static List<string> pictures = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }
        private void Start()
        {
            OSDefinition();
            ImportSettings();
            GetAllPictures();
            switch (Settings.Mode)
            {
                case 1:
                    MainImage.Source = pictures.Count > 0 ? new Bitmap(pictures.LastOrDefault()) : null;
                    break;
                case 2:
                    Dispatcher.UIThread.Post(() => SecondModeCycle(), DispatcherPriority.Background);
                    break;
                default:
                    new InfoWindow($"Режим не найден. ({Settings.Mode})").Show();
                    break;
            }
        }
        private void OSDefinition()
        {
            //new InfoWindow(Environment.OSVersion.ToString()).Show();
            if (Environment.OSVersion.ToString().Substring(0, 9) == "Microsoft")
                Settings.OS = "Windows";
            else if (Environment.OSVersion.ToString().Substring(0, 4) == "Unix")
                Settings.OS = "Unix";
        }
        private void ImportSettings()
        {
            try
            {
                string path = Environment.CurrentDirectory + "\\settings.ini";
                INIManagerV manager = new INIManagerV(path);

                //Импорт настроек из ini файла.
                Byte temp;
                Settings.Mode = Byte.TryParse(manager.GetPrivateString("mode"), out temp) ? temp : Settings.Mode;
                Settings.Rate = Byte.TryParse(manager.GetPrivateString("rate"), out temp) ? temp : Settings.Rate;
                Settings.PicturesDirectoryPath = manager.GetPrivateString("picdirectory").Trim() != string.Empty ? manager.GetPrivateString("mode") : Settings.PicturesDirectoryPath;
                Settings.Extensions = manager.GetPrivateString("ext") != string.Empty ? manager.GetPrivateString("ext").Split('/') : Settings.Extensions;
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
                    await Task.Delay(Settings.Rate * 1000);
                }
            }
        }
        private void GetAllPictures()
        {

            pictures.Clear();


            foreach (string file in Directory.EnumerateFiles(Settings.PicturesDirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(item => Settings.Extensions.Any(ext => '.' + ext == Path.GetExtension(item))))
            {
                pictures.Add(file);
            }

            //new InfoWindow(string.Join("\n", pictures)).Show();
        }
        private void OnImportButtonClick(object sender, RoutedEventArgs e)
        {
            //ImportSettings();
            //GetAllPictures();
            //Dispatcher.UIThread.Post(() => SecondModeCycle(), DispatcherPriority.Background);
            new InfoWindow("Current settings:\n" +
                $"OS:{Settings.OS}\n" +
                $"Mode:{Settings.Mode}\n" +
                $"Rate:{Settings.Rate}sec\n" +
                $"PictureDirectory:{Settings.PicturesDirectoryPath}\n" +
                $"FileExtensions:{string.Join("/", Settings.Extensions)}" +
                $"\n\n{string.Join("\n", pictures)}").Show();
        }
    }
}
