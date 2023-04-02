using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public string Temp { get; set; } = "qwe";
    public ObservableCollection<DisplayClass> Displays { get; set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        Displays.Add(new DisplayClass
        {
            DisplayNum = 1
        });
        Displays.Add(new DisplayClass
        {
            DisplayNum = 2
        });
        DataContext = this;
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

                SettingsDirectoryTextBox.Text = Environment.CurrentDirectory;

                Displays.Clear();
                Displays.Add(new DisplayClass
                {
                    DisplayNum = 1,
                    DirectoryPath = Environment.CurrentDirectory,
                    IsModeTwo = false
                });
            }
            else if (mode == "2")
            {
                ModeHintLabel.Content = "Карусель";
                FirstModeStackPanel.IsVisible = false;
                SecondModeStackPanel.IsVisible = true;
                
                Displays.Clear();
            }
        }
    }

    private void AddDisplayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //_displays.Add();
    }

    public class DisplayClass
    {
        public int DisplayNum { get; set; }
        public string? DirectoryPath { get; set; }
        public int? Rate { get; set; }
        public bool? IsModeTwo { get; set; }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        // Temp = "button";
        new InfoWindow(Temp).Show();
    }
}