using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
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
        private static string[] picturess = new string[100];
        public MainWindow()
        {
            InitializeComponent();
            Awake();
            Start();
        }
        private void Awake()
        {
            OSDefinition();
            ImportSettings();
        }
        private void Start()
        {
            if (Settings.Mode == 1)
            {

            }
            else if (Settings.Mode == 2)
            {
                GetAllPictures();
                PictureUpdater();
            }
            else
            {
                new InfoWindow("Режим не найден.").Show();
            }


            //new TempWindow().Show();
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
            if (Settings.OS == "Windows")
            {

                string pathWin = Environment.CurrentDirectory + "\\settings.ini";
                INIManagerV manager = new INIManagerV(pathWin);

                Settings.Mode = Int32.Parse(manager.GetPrivateString("mode"));
                Settings.Rate = Int32.Parse(manager.GetPrivateString("rate"));
                //Settings.PicturesDirectoryPath = manager.GetPrivateString("picdirectory");

                //new InfoWindow(Settings.Mode.ToString()).Show();

            }
            else if (Settings.OS == "Unix")
            {
                return;
            }
            else
            {
                new InfoWindow(Environment.OSVersion.ToString()).Show();
                return;
            }

        }
        private void ExportSettings()
        {

        }
        private void PictureUpdater()
        {
            Dispatcher.UIThread.Post(() => LongRunningTask(), DispatcherPriority.Background);
        }
        private async Task LongRunningTask()
        {
            while (true)
            {
                foreach (var item in pictures)
                {
                    MainImage.Source = new Bitmap(item);
                    await Task.Delay(Settings.Rate*1000);
                }

            }
        }
        private void GetAllPictures()
        {
            string str = "";

            pictures.Clear();

            string[] extensions = { ".png", ".jpeg", ".jpg", ".wav", ".bmp", ".tiff", ".jfif" };

            foreach (string file in Directory.EnumerateFiles(Settings.PicturesDirectoryPath, "*.*", SearchOption.AllDirectories)
                .Where(item => extensions.Any(ext => ext == System.IO.Path.GetExtension(item))))
            {
                pictures.Add(file);
                str += file + "\n";
            }

            new InfoWindow(str).Show();
        }
        private void OnImportButtonClick(object sender, RoutedEventArgs e)
        {
            //ImportSettings();
            //GetAllPictures();
            PictureUpdater();
        }
    }
}
