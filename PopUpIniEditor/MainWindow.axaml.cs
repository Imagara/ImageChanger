using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PopUpIniEditor;

public partial class MainWindow : Window
{
    private FileInfo _lauchIniFileInfo;
    private List<String> _imagesPaths = new();
    private List<Image> _images = new();
    private List<DisplayClass> _displays = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ChooseIniFileButton_OnClick(object? sender, RoutedEventArgs e)
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
                _lauchIniFileInfo = iniFile;
            }
        }
    }

    private async void ChooseFilesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog op = new OpenFileDialog();
        op.Title = "Выбрать файлы";
        op.Filters!.Add(new FileDialogFilter()
            { Name = "Images", Extensions = { "png", "jpeg", "jpg", "jfif", "webm" } });
        op.AllowMultiple = true;
        var result = await op.ShowAsync(this);
        if (result != null)
        {
            foreach (var item in result)
            {
                FileInfo file = new FileInfo(item);
                if (file.Exists)
                {
                    _imagesPaths.Add(item);
                }
            }
        }
    }

    private void ModeSelectComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox)
        {
            string mode = ((Label)((ComboBox)sender).SelectedItem).Content.ToString();
            if (mode == "1")
            {
                ModeHintLabel.Content = "Всплывашка";
                FirstModeStackPanel.IsVisible = true;
                SecondModeStackPanel.IsVisible = false;
                _displays.Clear();
                _displays.Add(new DisplayClass
                {
                    DisplayNum = 1,
                    DirectoryPath = SettingsDirectoryTextBox.Text
                });
            }
            else if (mode == "2")
            {
                ModeHintLabel.Content = "Карусель";
                FirstModeStackPanel.IsVisible = false;
                SecondModeStackPanel.IsVisible = true;
            }
        }
    }

    private void AddDisplayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //_displays.Add();
    }

    class DisplayClass
    {
        public int DisplayNum;
        public string? DirectoryPath;
        public int? Rate;
    }
}