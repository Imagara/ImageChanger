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

namespace ImageChanger
{
    public partial class MainWindow : Window
    {
        private List<string> pictures = new List<string>();
        private int displayNum = 1;
        public Settings settings = new();
        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        public MainWindow(int aDisplayNum)
        {
            InitializeComponent();
            displayNum = aDisplayNum;
            Start();
        }
        private void Start()
        {
            this.Title = $"Display #{displayNum}"; //Test
            ImportSettings(); 
            GetAllPictures();
            switch (settings.Mode)
            {
                case 1:
                    MainImage.Source = pictures.Count > 0 ? new Bitmap(pictures.LastOrDefault()) : null;
                    break;
                case 2:
                    Dispatcher.UIThread.Post(() => SecondModeCycle(), DispatcherPriority.Background);
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
                string path;
                if (MainSettings.OS == "Windows")
                    path = Environment.CurrentDirectory + "\\settings.ini";
                else if (MainSettings.OS == "Unix")
                    path = Environment.CurrentDirectory + "/settings.ini";
                else
                    return;
                INIManagerV manager = new INIManagerV(path);

                //Импорт настроек из ini файла.
                Byte temp;
                settings.Mode = Byte.TryParse(manager.GetPrivateString($"display{displayNum}", "mode"), out temp) ? temp : settings.Mode;
                settings.Rate = Byte.TryParse(manager.GetPrivateString($"display{displayNum}", "rate"), out temp) ? temp : settings.Rate;
                settings.PicturesDirectoryPath = manager.GetPrivateString($"display{displayNum}", "picdirectory").Trim() != string.Empty ? manager.GetPrivateString($"display{displayNum}", "picdirectory") : settings.PicturesDirectoryPath;
                settings.Extensions = manager.GetPrivateString($"display{displayNum}", "ext") != string.Empty ? manager.GetPrivateString($"display{displayNum}", "ext").Split('/') : settings.Extensions;
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
                $"OS:{MainSettings.OS}\n" +
                $"Mode:{settings.Mode}\n" +
                $"Rate:{settings.Rate}sec\n" +
                $"PictureDirectory:{settings.PicturesDirectoryPath}\n" +
                $"FileExtensions:{string.Join("/", settings.Extensions)}" +
                $"\n\n{string.Join("\n", pictures)}").Show();
        }
    }
}
