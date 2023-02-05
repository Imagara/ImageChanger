using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using static ImageChanger.MainWindow;
using System;
using Avalonia.Platform;
using Avalonia.Controls;
using System.Collections.Generic;
using System.IO;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Drawing.Text;

namespace ImageChanger
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                OSDefinition();
                StartUp st = new();
                st.ShowWindow();
                //desktop.MainWindow = new MainWindow(1);
                //MainWindow mw = new(2);
                //mw.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
        private void OSDefinition()
        {
            if (Environment.OSVersion.ToString().Substring(0, 9) == "Microsoft")
                MainSettings.OS = "Windows";
            else if (Environment.OSVersion.ToString().Substring(0, 4) == "Unix")
                MainSettings.OS = "Unix";
        }
    }
}
