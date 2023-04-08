using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using PopUpIniEditorMVVM.Views;
using ReactiveUI;

namespace PopUpIniEditorMVVM.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public ReactiveCommand<Unit, Unit> AddDisplayCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveDisplayCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateSettingsIniFileCommand { get; }
    public ReactiveCommand<Window, Unit> DirectorySelectCommand { get; }

    public MainWindowViewModel()
    {
        AddDisplayCommand = ReactiveCommand.Create(AddDisplay);
        RemoveDisplayCommand = ReactiveCommand.Create(RemoveDisplay);
        UpdateSettingsIniFileCommand = ReactiveCommand.Create(UpdateSettingsIniFile);
        DirectorySelectCommand = ReactiveCommand.Create<Window>(DirectorySelect);
    }

    public void UpdateSettingsIniFile()
    {
        string? selectedModeContent = ((Label)_selectedMode).Content.ToString();
        int.TryParse(selectedModeContent, out var mode);
        if (mode is 1 or 2)
        {
            List<string> args = new();
            args.Add("[main]\n" +
                     $"mode={mode}");
            if (mode is 1)
            {
                args.Add($"directory={_settingsDirectoryContent}");
            }
            else
            {
                var screens = _displays.Select(item => item.DisplayNum).ToList();
                args.Add($"screens={string.Join("/", screens)}");
                foreach (var display in _displays)
                {
                    args.Add($"[display{display.DisplayNum}]\n" +
                             $"directory={display.DirectoryPath}\n" +
                             $"rate={display.Rate}\n");
                }
            }

            File.WriteAllLines("settings.ini", args);
            
            //need to test
            Process PrFolder = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            string file = Environment.CurrentDirectory + Slash + "settings.ini";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.FileName = "explorer";
            psi.Arguments = @"/n, /select, " + file;
            PrFolder.StartInfo = psi;
            PrFolder.Start();
        }
    }

    public void AddDisplay()
    {
        int.TryParse(SelectedDisplay, out var display);

        if (display > 0 && Displays.All(item => item.DisplayNum != display))
        {
            Displays.Add(new DisplayClass
            {
                DisplayNum = display
            });
        }
    }
    public async void DirectorySelect(Window window)
    {
        OpenFolderDialog ofd = new OpenFolderDialog();
        ofd.Title = "Select folder";
        var result = await ofd.ShowAsync(window);
        if (result != null)
            SettingsDirectoryContent = result;
    }

    public void RemoveDisplay()
    {
        int.TryParse(SelectedDisplay, out var display);

        if (Displays.Any(item => item.DisplayNum == display))
            Displays.Remove(Displays.First(item => item.DisplayNum == display));
    }

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

    private string _settingsDirectoryContent;

    public string SettingsDirectoryContent
    {
        get => _settingsDirectoryContent;
        set
        {
            _settingsDirectoryContent = value;
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

    private void ModeChanged()
    {
        string? selectedModeContent = ((Label)_selectedMode).Content.ToString();
        int.TryParse(selectedModeContent, out var mode);

        if (mode == 1)
        {
            ModeHint = "Всплывашка";
            IsFirstModeStackPanelVisible = true;
            IsSecondModeStackPanelVisible = false;

            Displays.Clear();
            Displays.Add(new DisplayClass
            {
                DisplayNum = 1,
                IsModeTwo = false
            });
        }
        else if (mode == 2)
        {
            ModeHint = "Карусель";

            IsFirstModeStackPanelVisible = false;
            IsSecondModeStackPanelVisible = true;

            Displays.Clear();
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
        public int? Rate { get; set; }
        public bool? IsModeTwo { get; set; }
    }
}