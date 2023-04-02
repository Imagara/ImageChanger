using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public MainWindowViewModel()
    {
        AddDisplayCommand = ReactiveCommand.Create(AddDisplay);
        RemoveDisplayCommand = ReactiveCommand.Create(RemoveDisplay);
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

    private ObservableCollection<DisplayClass> _displays = new()
    {
        new DisplayClass
        {
            DisplayNum = 1
        }
    };

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
                DirectoryPath = Environment.CurrentDirectory,
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


    private string _greetingsText;

    public string GreetingsText
    {
        get => _greetingsText;
        set
        {
            _greetingsText = value;
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
        public int? Rate { get; set; }
        public bool? IsModeTwo { get; set; }
    }
}