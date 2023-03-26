using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PopUpIniEditor;

public partial class MainWindow : Window
{
    private FileInfo lauchIniFileInfo;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenIniFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog op = new OpenFileDialog();
        op.Title = "Выбрать конфигурационный файл";
        op.Filters!.Add(new FileDialogFilter() { Name = "Ini Files", Extensions = { "ini" } });
        op.AllowMultiple = false;
        var result = await op.ShowAsync(this);
        if (result != null)
        {
            FileInfo iniFile = new FileInfo(result.First());
            if (iniFile.Exists && iniFile.Extension == ".ini")
            {
                SelectedIniLabel.Content = iniFile.Name;
                lauchIniFileInfo = iniFile;
            }
        }
    }
}