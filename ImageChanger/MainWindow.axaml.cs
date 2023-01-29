using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Interactivity;

namespace ImageChanger
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Awake();
            Start();
        }
        void Awake()
        {
            OSDefinition();
            ImportSettings();
        }
        void Start()
        {
            //new TempWindow().Show();
        }
        void OSDefinition()
        {
            new InfoWindow(Environment.OSVersion.ToString()).Show();
            if (Environment.OSVersion.ToString().Substring(0, 9) == "Microsoft")
                Settings.OS = "Windows";
            
        }
        void ImportSettings()
        {
            if (Settings.OS == "Windows")
            {

                string pathWin = Environment.CurrentDirectory + "\\settings.ini";
                INIManagerV manager = new INIManagerV(pathWin);

                Settings.Mode = Int32.Parse(manager.GetPrivateString("mode"));

                new InfoWindow(Settings.Mode.ToString()).Show();

            }
            else
            {
                new InfoWindow("Not exists" + " curr dir: " + Environment.CurrentDirectory.ToString()).Show();
                return;
            }

        }
        void ExportSettings()
        {

        }
        private void OnImportButtonClick(object sender, RoutedEventArgs e)
        {
            ImportSettings();
        }
    }
}
