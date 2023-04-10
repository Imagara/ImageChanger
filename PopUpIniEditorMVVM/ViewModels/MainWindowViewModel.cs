using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using PopUpIniEditorMVVM.Views;
using ReactiveUI;

namespace PopUpIniEditorMVVM.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    // Event to signal ViewModel property changes to the View and update the UI display
    public event PropertyChangedEventHandler PropertyChanged;

    public ReactiveCommand<Unit, Unit> AddDisplayCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveDisplayCommand { get; }
    public ReactiveCommand<Window, Unit> AddAnnouncementCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveAnnouncementCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateSettingsIniFileCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateLaunchIniFileCommand { get; }
    public ReactiveCommand<Window, Unit> DirectorySelectCommand { get; }
    public ReactiveCommand<Window, Unit> LaunchSelectCommand { get; }

    // Constructor creating each user command and associated action
    public MainWindowViewModel()
    {
        AddDisplayCommand = ReactiveCommand.Create(AddDisplay);
        RemoveDisplayCommand = ReactiveCommand.Create(RemoveDisplay);
        AddAnnouncementCommand = ReactiveCommand.Create<Window>(AddAnnouncement);
        RemoveAnnouncementCommand = ReactiveCommand.Create(RemoveAnnouncement);
        UpdateSettingsIniFileCommand = ReactiveCommand.Create(UpdateSettingsIniFile);
        UpdateLaunchIniFileCommand = ReactiveCommand.Create(UpdateLaunchIniFile);
        DirectorySelectCommand = ReactiveCommand.Create<Window>(DirectorySelect);
        LaunchSelectCommand = ReactiveCommand.Create<Window>(LaunchSelect);
    }

    private void UpdateLaunchIniFile()
    {
        string path = _launchPath;

        List<string> strs = _announcements
            .Select(item => $"{item.ImagePath}|{item.LastWriteTime}|{item.ActualStart}|{item.ActualEnd}").ToList();

        File.WriteAllLines(path, strs);
    }

    // Method to write out changes made to settings.ini
    private void UpdateSettingsIniFile()
    {
        if (_selectedMode is not Label modeLabel)
            return;

        int.TryParse(modeLabel.Content.ToString(), out var mode);

        if (mode is 1 or 2)
        {
            List<string> strs = new()
            {
                "[main]\n" +
                $"mode={mode}"
            };
            if (mode is 1)
            {
                strs.Add($"directory={_directory}");
            }
            else
            {
                var screens = _displays.Select(item => item.DisplayNum).ToList();
                strs.Add($"screens={string.Join("/", screens)}");
                foreach (var display in _displays)
                {
                    strs.Add($"[display{display.DisplayNum}]\n" +
                             $"directory={display.DirectoryPath}\n" +
                             $"rate={display.Rate}\n");
                }
            }

            // Write the updated settings.ini file
            File.WriteAllLines("settings.ini", strs);

            //need to test
            Process PrFolder = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            string file = Path.Combine(Environment.CurrentDirectory, "settings.ini");
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.FileName = "explorer";
            psi.Arguments = @"/n, /select, " + file;
            PrFolder.StartInfo = psi;
            PrFolder.Start();
        }
    }

    private void ModeChanged()
    {
        if (_selectedMode is not Label label)
            return;

        bool isFirstMode = label.Content.ToString() == "1";

        ModeHint = isFirstMode ? "Всплывашка" : "Карусель";

        IsFirstModeStackPanelVisible = isFirstMode;
        IsSecondModeStackPanelVisible = !isFirstMode;

        if (isFirstMode)
        {
            Displays.Clear();
            Displays.Add(new DisplayClass { DisplayNum = 1, IsModeTwo = false });
        }
        else
        {
            Displays.Clear();
        }
    }


    private async void DirectorySelect(Window window)
    {
        OpenFolderDialog ofd = new OpenFolderDialog();
        ofd.Title = "Select folder";
        var result = await ofd.ShowAsync(window);
        if (result != null)
            Directory = result;
    }

    private async void LaunchSelect(Window window)
    {
        OpenFileDialog op = new OpenFileDialog();

        op.Title = "Выбрать конфигурационный файл";
        op.Filters!.Add(new FileDialogFilter() { Name = "Ini Files", Extensions = { "ini" } });
        op.AllowMultiple = false;

        var result = await op.ShowAsync(window);
        if (result != null)
            LaunchPath = result.First();
    }

    private void ImportAnnouncements(string path)
    {
        FileInfo iniFile = new FileInfo(path);
        if (iniFile.Exists && iniFile.Extension == ".ini")
        {
            var announcementsStrs = File.ReadLines(iniFile.FullName);
            
            _announcements.Clear();
            foreach (var item in announcementsStrs)
            {
                Regex regex = new Regex(
                    @"^[A-Za-z0-9.:\\/-]{4,128}[|][0-9.:\s]{10,19}[|][0-9.:\s]{10,19}[|][0-9.:\s]{10,19}",
                    RegexOptions.Compiled);

                if (regex.IsMatch(item))
                {
                    string[] subs = item.Split('|');
                    
                    DateTime lastWriteTime;
                    DateTime actualStart;
                    DateTime actualEnd;

                    if (!DateTime.TryParse(subs[1], out lastWriteTime)
                        || !DateTime.TryParse(subs[2], out actualStart)
                        || !DateTime.TryParse(subs[3], out actualEnd))
                        continue;
                    _announcements.Add(new AnnouncementClass
                    {
                        Name = subs[0],
                        ImagePath = subs[0].IndexOf(':') == -1 ? _launchPath + subs[0] : subs[0],
                        LastWriteTime = lastWriteTime,
                        ActualStart = actualStart,
                        ActualEnd = actualEnd
                    });
                }
            }

            OnPropertyChanged(nameof(AnnouncementCountContent));
        }
        else if (iniFile.Extension == ".ini")
            LaunchPath = iniFile.FullName;
    }

    private void AddDisplay()
    {
        int.TryParse(SelectedDisplay, out var displayNum);

        if (displayNum > 0 && Displays.All(item => item.DisplayNum != displayNum))
        {
            Displays.Add(new DisplayClass
            {
                DisplayNum = displayNum
            });
        }
    }

    private void RemoveDisplay()
    {
        int.TryParse(SelectedDisplay, out var display);

        if (Displays.Any(item => item.DisplayNum == display))
            Displays.Remove(Displays.First(item => item.DisplayNum == display));
    }

    private async void AddAnnouncement(Window window)
    {
        OpenFileDialog op = new OpenFileDialog();

        op.Title = "Выбрать обьявление";
        op.Filters!.Add(new FileDialogFilter
            { Name = "Изображения", Extensions = { "png", "jpeg", "jpg" } });
        op.AllowMultiple = true;

        var result = await op.ShowAsync(window);
        if (result != null)
        {
            foreach (var item in result)
            {
                FileInfo imageFileInfo = new FileInfo(item);
                if (imageFileInfo.Exists)
                {
                    Announcements.Add(new AnnouncementClass
                    {
                        Name = imageFileInfo.Name,
                        ImagePath = item,
                        LastWriteTime = imageFileInfo.LastWriteTime
                    });
                }
            }
        }

        OnPropertyChanged(nameof(AnnouncementCountContent));
    }

    private void RemoveAnnouncement()
    {
    }

    public string AnnouncementCountContent => $"Обьявлений : {_announcements.Count}";

    private Object _selectedMode = 1;

    public Object SelectedMode
    {
        get => _selectedMode;
        set
        {
            _selectedMode = value;
            OnPropertyChanged();
            ModeChanged();
        }
    }

    private bool _isFirstModeStackPanelVisible;

    public bool IsFirstModeStackPanelVisible
    {
        get => _isFirstModeStackPanelVisible;
        set
        {
            _isFirstModeStackPanelVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _isSecondModeStackPanelVisible;

    public bool IsSecondModeStackPanelVisible
    {
        get => _isSecondModeStackPanelVisible;
        set
        {
            _isSecondModeStackPanelVisible = value;
            OnPropertyChanged();
        }
    }

    private string _directory;

    public string Directory
    {
        get => _directory;
        set
        {
            _directory = value;
            OnPropertyChanged();
        }
    }

    private string _launchPath;

    public string LaunchPath
    {
        get => _launchPath;
        set
        {
            _launchPath = value;
            ImportAnnouncements(_launchPath);
            OnPropertyChanged();
        }
    }

    private ObservableCollection<DisplayClass> _displays = new();

    public ObservableCollection<DisplayClass> Displays
    {
        get => _displays;
        set
        {
            _displays = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<AnnouncementClass> _announcements = new();

    public ObservableCollection<AnnouncementClass> Announcements
    {
        get => _announcements;
        set
        {
            _announcements = value;
            OnPropertyChanged();
        }
    }

    private string _modeHint;

    public string ModeHint
    {
        get => _modeHint;
        set
        {
            _modeHint = value;
            OnPropertyChanged();
        }
    }

    private string _updateSettingsIniFileContent = "Создать";

    public string UpdateSettingsIniFileContent
    {
        get => _updateSettingsIniFileContent;
        set
        {
            _updateSettingsIniFileContent = value;
            OnPropertyChanged();
        }
    }

    private string _selectedDisplay;

    public string SelectedDisplay
    {
        get => _selectedDisplay;
        set
        {
            _selectedDisplay = value;
            OnPropertyChanged();
        }
    }

    public void OnPropertyChanged([CallerMemberName] string property = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public class DisplayClass
    {
        public int DisplayNum { get; set; }
        public string? DirectoryPath { get; set; }
        public int? Rate { get; set; } = 60;
        public bool? IsModeTwo { get; set; }
    }

    public class AnnouncementClass
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime? ActualStart { get; set; } = DateTime.Now;
        public DateTime? ActualEnd { get; set; } = DateTime.MaxValue;
    }
}