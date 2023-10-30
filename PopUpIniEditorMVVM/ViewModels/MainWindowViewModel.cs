using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using PopUpIniEditorMVVM.Views;
using ReactiveUI;

namespace PopUpIniEditorMVVM.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    private static Encoding UTF8NoBOM =>
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private static List<Window> Windows { get; set; } = new();

    private List<ActionClass> _actionsList = new();
    private List<ActionClass> _redoList = new List<ActionClass>();


    // Event to signal ViewModel property changes to the View and update the UI display
    public event PropertyChangedEventHandler PropertyChanged;

    #region ReactiveCommands

    public ReactiveCommand<string, Unit> ReplaceAnnouncementCommand { get; }
    public ReactiveCommand<Unit, Unit> AddDisplayCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveDisplayCommand { get; }
    public ReactiveCommand<Window, Unit> AddAnnouncementCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveAnnouncementCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateSettingsIniFileCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateLaunchIniFileCommand { get; }
    public ReactiveCommand<Window, Unit> DirectorySelectCommand { get; }
    public ReactiveCommand<Window, Unit> LaunchSelectCommand { get; }
    public ReactiveCommand<Window, Unit> LaunchSelectDirectoryCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenScreenDefinitionHelpWindowsCommand { get; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; }

    #endregion

    // Constructor creating each user command and associated action
    public MainWindowViewModel()
    {
        ReplaceAnnouncementCommand = ReactiveCommand.Create<string>(ReplaceAnnouncementChangeAnswer);
        AddDisplayCommand = ReactiveCommand.Create(AddDisplay);
        RemoveDisplayCommand = ReactiveCommand.Create(RemoveDisplay);
        AddAnnouncementCommand = ReactiveCommand.Create<Window>(AddAnnouncementOpenDialog);
        RemoveAnnouncementCommand = ReactiveCommand.Create(RemoveAnnouncementFromList);
        UpdateSettingsIniFileCommand = ReactiveCommand.Create(UpdateSettingsIniFile);
        UpdateLaunchIniFileCommand = ReactiveCommand.Create(UpdateLaunchIniFile);
        DirectorySelectCommand = ReactiveCommand.Create<Window>(DirectorySelect);
        LaunchSelectCommand = ReactiveCommand.Create<Window>(LaunchSelect);
        LaunchSelectDirectoryCommand = ReactiveCommand.Create<Window>(LaunchDirectorySelect);
        OpenScreenDefinitionHelpWindowsCommand = ReactiveCommand.Create(OpenScreenDefinitionHelpWindows);
        UndoCommand = ReactiveCommand.Create(Undo);
    }


    void Undo() // alt + z
    {
        if (_actionsList.Count <= 0)
            return;

        ActionClass action = _actionsList.Last();

        switch (action.Action)
        {
            case "add_display":
                if (_displays.Select(item => item.DisplayNum).Contains(int.Parse(action.Args[0])))
                {
                    Displays.Remove(_displays.Where(item => item.DisplayNum == int.Parse(action.Args[0])).First());
                }

                break;
            case "remove_display":
                try
                {
                    Displays.Add(new DisplayClass
                    {
                        DisplayNum = int.Parse(action.Args[0]),
                        DirectoryPath = action.Args[1],
                        Rate = int.Parse(action.Args[2]),
                        IsModeTwo = Boolean.Parse(action.Args[3])
                    });
                }
                catch (Exception e)
                {
                    //
                }

                break;
            case "remove_announcement":
                try
                {
                    Announcements.Add(new AnnouncementClass
                    {
                        Name = action.Args[0],
                        Extension = action.Args[1],
                        Path = action.Args[2],
                        LastWriteTime = DateTime.Parse(action.Args[3]),
                        ActualStart = DateTime.Parse(action.Args[4]),
                        ActualEnd = DateTime.Parse(action.Args[5]),
                    });
                }
                catch (Exception e)
                {
                    //
                }

                break;
            case "add_announcement":
                Announcements.Remove(_announcements
                    .Where(item => item.Name == action.Args[0] && item.Extension == action.Args[1]).First());
                break;
            default:
                break;
        }

        _actionsList.Remove(action);
    }

    void Redo() // alt + shift + z
    {
    }

    private void ReplaceAnnouncementChangeAnswer(string param)
    {
        _replaceAnnouncementAnswer = param;
    }

    private void UpdateLaunchIniFile()
    {
        try
        {
            if (_launchPath == null)
            {
                ShowInfoMessage("Не выбран путь к конфигурационному файлу", "danger");
                return;
            }

            List<string> strs = new();

            strs.Add($"autodelete={_launchAutoDeleteFile}\n");

            ActionClass action;

            foreach (var item in _actionsList)
            {
                action = item;

                if (action == null)
                    continue;

                switch (action.Action)
                {
                    case "add_announcement":
                        try
                        {
                            string newPath = Path.Combine(GetLaunchPath(),
                                action.Args[0] ?? throw new InvalidOperationException("Name undefined"));
                            string? oldPath = action.Args[2];

                            if (oldPath == null)
                                throw new Exception("old path (arg[2]) undefined");

                            FileInfo fileToCheck = new FileInfo(oldPath);

                            if (!fileToCheck.Exists)
                                throw new Exception($"Файл больше не существует ({action.Args[0]})");

                            if (action.Args[3] != fileToCheck.LastWriteTime.ToString(CultureInfo.CurrentCulture))
                                throw new Exception($"Not the same file ({action.Args[0]})");

                            if (newPath != oldPath)
                                File.Copy(action.Args[2]!, newPath);
                        }
                        catch (Exception e)
                        {
                            ShowInfoMessage($"Ошибка при перемещении файла {action.Args[0]}. {e.Message}", "Danger");
                        }

                        break;
                    case "remove_announcement":
                        try
                        {
                            string path = action.Args[2];
                            FileInfo annofile = new FileInfo(path);
                            if (!annofile.Exists)
                                break;

                            File.Delete(path);
                            Task.Delay(50);
                        }
                        catch
                        {
                            ShowInfoMessage($"Ошибка при удалении файла {action.Args[0]}", "Danger");
                        }

                        break;
                }
            }

            _actionsList.Clear();

            strs.AddRange(_announcements
                .Select(item => $"{item.Name}|{item.LastWriteTime}|{item.ActualStart}|{item.ActualEnd}").ToList());

            File.WriteAllLines(_launchPath, strs, UTF8NoBOM);

            ShowInfoMessage("Сохранено.", "success");
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при сохранении конфигурационного файла: " + e.Message, "danger");
        }
    }

    // Method to write out changes made to settings.ini
    private void UpdateSettingsIniFile()
    {
        try
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
                    strs.Add($"directory={_directory}\n" +
                             $"activitystart={_activityStart}\n" +
                             $"activityend={_activityEnd}");
                }
                else
                {
                    var screens = _displays.Select(item => item.DisplayNum).ToList();
                    strs.Add($"isblackout={_isBlackout}\n" +
                             $"blackoutstart={_blackoutStart}\n" +
                             $"blackoutend={_blackoutEnd}\n" +
                             $"background={_hexCodeColor}\n" +
                             $"screens={string.Join("/", screens)}\n");
                    foreach (var display in _displays)
                    {
                        strs.Add($"[display{display.DisplayNum}]\n" +
                                 $"directory={display.DirectoryPath}\n" +
                                 $"rate={display.Rate}\n");
                    }
                }

                // Write the updated settings.ini file
                File.WriteAllLines("settings.ini", strs, UTF8NoBOM);

                SelectUpdatedFile(Path.Combine(Environment.CurrentDirectory, "settings.ini"));

                ShowInfoMessage("Сохранено.", "success");
            }
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при сохранении конфигурационного файла: " + e.Message, "danger");
        }
    }

    private void SelectUpdatedFile(string path)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using Process PrFolder = new Process();
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = "explorer",
                    Arguments = @"/n, /select, " + path
                };
                PrFolder.StartInfo = psi;
                PrFolder.Start();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using Process dbusShowItemsProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dbus-send",
                        Arguments =
                            "--print-reply --dest=org.freedesktop.FileManager1 /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://" +
                            path + "\" string:\"\"",
                        UseShellExecute = true
                    }
                };
                dbusShowItemsProcess.Start();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using Process fileOpener = new Process();
                fileOpener.StartInfo.FileName = "explorer";
                fileOpener.StartInfo.Arguments = "-R " + path;
                fileOpener.Start();
            }
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при открытии папки: " + e.Message, "danger");
        }
    }

    private void ModeChanged()
    {
        try
        {
            if (_selectedMode is not Label modeLabel)
                return;

            bool isFirstMode = modeLabel.Content.ToString() == "1";

            ModeHint = isFirstMode ? "Отображает доступные объявления на экран" : "Циклично показывает изображения";

            IsFirstModeStackPanelVisible = isFirstMode;
            IsSecondModeStackPanelVisible = !isFirstMode;

            Displays.Clear();

            if (isFirstMode)
                Displays.Add(new DisplayClass { DisplayNum = 1, IsModeTwo = false });
            else
                Displays.Add(new DisplayClass { DisplayNum = 1, IsModeTwo = true });
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при выборе режима работы программы: " + e.Message, "danger");
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
        op.Filters!.Add(new FileDialogFilter { Name = "Ini Files", Extensions = { "ini" } });
        op.AllowMultiple = false;

        var result = await op.ShowAsync(window);
        if (result != null)
            LaunchPath = result.First();
    }

    private async void LaunchDirectorySelect(Window window)
    {
        OpenFolderDialog ofd = new OpenFolderDialog();
        ofd.Title = "Select folder";
        var result = await ofd.ShowAsync(window);
        if (result != null)
            LaunchPath = Path.Combine(result, "launch.ini");
    }

    private void ImportAnnouncements(string path)
    {
        _announcements.Clear();

        if (path.Trim() == "")
            return;

        try
        {
            FileInfo iniFile = new FileInfo(path);

            OnPropertyChanged(nameof(AnnouncementCountContent));

            if (iniFile.Exists && iniFile.Extension == ".ini")
            {
                var announcementsStrs = File.ReadLines(iniFile.FullName).ToList();

                foreach (var item in announcementsStrs)
                {
                    Regex announcementRegex = new Regex(
                        @"^[A-Za-zА-Яа-я0-9.() —:\\/-]{4,128}[|][0-9.:\s]{10,19}[|][0-9.:\s]{10,19}[|][0-9.:\s]{10,19}",
                        RegexOptions.Compiled);

                    if (!announcementRegex.IsMatch(item))
                        continue;

                    string[] subs = item.Split('|');

                    if (!DateTime.TryParse(subs[1], out var lastWriteTime)
                        || !DateTime.TryParse(subs[2], out var actualStart)
                        || !DateTime.TryParse(subs[3], out var actualEnd))
                    {
                        ShowInfoMessage("Ошибка получения дат при импорте объявлений", "danger");
                        continue;
                    }

                    _announcements.Add(new AnnouncementClass
                    {
                        Name = subs[0],
                        Path = Path.Combine(iniFile.Directory!.ToString(), subs[0]),
                        LastWriteTime = lastWriteTime,
                        ActualStart = actualStart,
                        ActualEnd = actualEnd
                    });
                }

                var autoDeleteLine = announcementsStrs.FirstOrDefault(s => s.StartsWith("autodelete="));
                if (autoDeleteLine != null)
                {
                    bool.TryParse(autoDeleteLine.Substring("autodelete=".Length), out var autoDeleteFiles);
                    LaunchAutoDeleteFile = autoDeleteFiles;
                }

                OnPropertyChanged(nameof(AnnouncementCountContent));
            }
            else if (iniFile.Extension == ".ini")
                _launchPath = iniFile.FullName;
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при импорте объявлений: " + e.Message, "danger");
        }
    }

    private void AddDisplay()
    {
        try
        {
            int.TryParse(SelectedDisplayContent, out var displayNum);

            if (displayNum > 0 && Displays.All(item => item.DisplayNum != displayNum))
            {
                Displays.Add(new DisplayClass
                {
                    DisplayNum = displayNum
                });

                _actionsList.Add(new ActionClass
                {
                    Action = "add_display",
                    Args = new[]
                    {
                        displayNum.ToString()
                    }
                });
            }
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при добавлении экрана для вывода: " + e.Message, "danger");
        }
    }

    private void RemoveDisplay()
    {
        try
        {
            int.TryParse(SelectedDisplayContent, out var selectedDisplay);

            if (_displays.Any(item => item.DisplayNum == selectedDisplay))
            {
                DisplayClass display = _displays.First(item => item.DisplayNum == selectedDisplay);

                _actionsList.Add(new ActionClass
                {
                    Action = "remove_display",
                    Args = new[]
                    {
                        display.DisplayNum.ToString(),
                        display.DirectoryPath,
                        display.Rate.ToString(),
                        display.IsModeTwo.ToString()
                    }
                });

                Displays.Remove(display);
            }
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при удалении: " + e.Message, "danger");
        }
    }

    private void OpenScreenDefinitionHelpWindows()
    {
        try
        {
            var screenHelper = new ScreenHelper();

            int index = 1;

            foreach (var item in ScreenHelper.AllScreens)
            {
                var xPos = item.Bounds.Position.X;

                ScreenDefinitionHelpWindow window = new ScreenDefinitionHelpWindow(index)
                {
                    Position = new PixelPoint(xPos, item.Bounds.Y)
                };

                Windows.Add(window);

                window.Show();

                index++;
            }

            CloseScreenDefinitionHelpWindows();
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при открытии окон: " + e.Message, "danger");
        }
    }

    private async void CloseScreenDefinitionHelpWindows()
    {
        await Task.Delay(3000);
        foreach (var item in Windows)
        {
            item.Close();
        }
    }

    private string GetHexCode(string value)
    {
        string str = value;
        if (str.Length < 6
            || (str.Length == 6 && str[0] == '#')
            || (str.Length == 7 && str[0] != '#')
            || str.Length > 7)
            return "#000000";
        if (str[0] != '#')
            str = '#' + str;
        return str;
    }

    private async void AddAnnouncementOpenDialog(Window window)
    {
        if (_launchPath == null)
        {
            ShowInfoMessage("Выберите путь к конфигурационному файлу", "danger");
            return;
        }

        OpenFileDialog op = new OpenFileDialog
        {
            Title = "Выбрать обьявление"
        };

        op.Filters!.Add(new FileDialogFilter
            { Name = "Изображения", Extensions = { "png", "jpeg", "jpg" } });
        op.AllowMultiple = true;

        var result = await op.ShowAsync(window);
        if (result != null)
            AddAnnouncementsFromResult(result);

        OnPropertyChanged(nameof(AnnouncementCountContent));
    }

    private async void AddAnnouncementsFromResult(string[] result)
    {
        try
        {
            DirectoryInfo launchDirectoryInfo = new DirectoryInfo(GetLaunchPath());

            foreach (var item in result)
            {
                FileInfo imageFileInfo = new FileInfo(item);
                if (!imageFileInfo.Exists)
                    continue;

                if (_announcements.Any(it => it.Name == imageFileInfo.Name))
                {
                    IsReplaceAnnouncementOpened = true;

                    OldAnnouncementImage = Path.Combine(GetLaunchPath(),
                        _announcements.First(i => i.Name == imageFileInfo.Name).Name);
                    NewAnnouncementImage = item;

                    ReplaceFileName = imageFileInfo.Name;
                    ReplaceNewFileName = "";

                    while (IsReplaceAnnouncementOpened)
                    {
                        await Task.Delay(200);
                        switch (_replaceAnnouncementAnswer)
                        {
                            case "replace":
                                IsReplaceAnnouncementOpened = false;
                                _replaceAnnouncementAnswer = "none";

                                try
                                {
                                    if (Path.Combine(GetLaunchPath(),
                                            imageFileInfo.Name) ==
                                        _announcements.First(ann => ann.Name == imageFileInfo.Name).Name)
                                    {
                                        return;
                                    }

                                    RemoveAnnouncement(imageFileInfo.Name, imageFileInfo.Extension,
                                        imageFileInfo.LastWriteTime);
                                    AddAnnouncement(imageFileInfo.Name, imageFileInfo.Extension, item,
                                        imageFileInfo.LastWriteTime);
                                }
                                catch (Exception e)
                                {
                                    ShowInfoMessage("Ошибка во время замены объявления: " + e.Message, "danger");
                                }

                                break;
                            case "rename":
                                try
                                {
                                    if (_replaceNewFileName.Trim() == "")
                                    {
                                        _replaceAnnouncementAnswer = "none";
                                        ShowInfoMessage(
                                            "Некорректное название",
                                            "danger");
                                        break;
                                    }

                                    if (_replaceNewFileName.Length > 128)
                                    {
                                        _replaceAnnouncementAnswer = "none";
                                        ShowInfoMessage(
                                            "Слишком длинное название. Поле «Новое название» должно содержать менее 128 символов",
                                            "danger");
                                        break;
                                    }
                                    
                                    string extension = imageFileInfo.Extension;

                                    string pattern = @".(jpe?g|jpg|png)$";
                                    Match m = Regex.Match(_replaceNewFileName.Trim(), pattern);
                                    if (m.Success)
                                        extension = m.Value;
                                    else
                                        _replaceNewFileName += extension;

                                    
                                    if (_announcements.Any(ann => ann.Name == _replaceNewFileName))
                                    {
                                        _replaceAnnouncementAnswer = "none";
                                        ShowInfoMessage("Объявление с таким названием уже существует.", "danger");
                                        break;
                                    }
                                    
                                    IsReplaceAnnouncementOpened = false;
                                    _replaceAnnouncementAnswer = "none";

                                    
                                    AddAnnouncement(_replaceNewFileName, extension, item,
                                        imageFileInfo.LastWriteTime);
                                    break;
                                }
                                catch (Exception e)
                                {
                                    ShowInfoMessage("Ошибка при попытке переименования файла: " + e.Message, "danger");
                                    _replaceAnnouncementAnswer = "none";
                                    break;
                                }
                            case "skip":
                                IsReplaceAnnouncementOpened = false;
                                _replaceAnnouncementAnswer = "none";
                                break;
                        }
                    }
                }
                else
                {
                    AddAnnouncement(imageFileInfo.Name, imageFileInfo.Extension, item, imageFileInfo.LastWriteTime);
                }
            }
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при добавлении выбранных объявлений: " + e.Message, "danger");
        }
    }

    private void AddAnnouncement(string name, string extension, string path, DateTime lastWrite)
    {
        try
        {
            Announcements.Add(new AnnouncementClass
            {
                Name = name,
                Extension = extension,
                Path = path,
                LastWriteTime = lastWrite
            });

            _actionsList.Add(new ActionClass
            {
                Action = "add_announcement",
                Args = new[]
                {
                    name,
                    extension,
                    path,
                    lastWrite.ToString(CultureInfo.CurrentCulture)
                }
            });
            OnPropertyChanged(nameof(AnnouncementCountContent));
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка при добавлении объявления в список: " + e.Message, "danger");
        }
    }

    private void RemoveAnnouncement(string name, string extension, DateTime lastWriteTime)
    {
        AnnouncementClass announcement =
            _announcements.First(item =>
                item.Name == name && item.Extension == extension && item.LastWriteTime == lastWriteTime);

        if (announcement is null)
            return;

        try
        {
            _actionsList.Add(new ActionClass
            {
                Action = "remove_announcement",
                Args = new[]
                {
                    announcement.Name,
                    announcement.Extension,
                    Path.Combine(GetLaunchPath(), announcement.Name),
                    announcement.LastWriteTime.ToString(CultureInfo.CurrentCulture),
                    announcement.ActualStart.ToString(),
                    announcement.ActualEnd.ToString()
                }
            });

            Announcements.Remove(announcement);
            File.Delete(Path.Combine(GetLaunchPath(), announcement.Name));

            OnPropertyChanged(nameof(AnnouncementCountContent));
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка во время удаления объявления: " + e.Message, "danger");
        }
    }

    private void RemoveAnnouncementFromList()
    {
        if (_selectedAnnouncement is not AnnouncementClass announcement)
            return;

        try
        {
            _actionsList.Add(new ActionClass
            {
                Action = "remove_announcement",
                Args = new[]
                {
                    announcement.Name,
                    announcement.Extension,
                    Path.Combine(GetLaunchPath(), announcement.Name),
                    announcement.LastWriteTime.ToString(CultureInfo.CurrentCulture),
                    announcement.ActualStart.ToString(),
                    announcement.ActualEnd.ToString()
                }
            });

            Announcements.Remove(announcement);

            OnPropertyChanged(nameof(AnnouncementCountContent));
        }
        catch (Exception e)
        {
            ShowInfoMessage("Ошибка во время удаления объявления: " + e.Message, "danger");
        }
    }

    public string GetHexCodeByType(string type)
    {
        type = type.ToLower();
        switch (type)
        {
            case "primary":
                return "#5bc0de";
            case "danger":
                return "#bb2124";
            case "success":
                return "#22bb33";
            case "warning":
                return "#f0ad4e";
            default:
                return "#000000";
        }
    }

    private void ShowInfoMessage(string message, string type = "Primary")
    {
        int info_id = _infoMessages.Select(item => item.Id).DefaultIfEmpty(0).Max() + 1;
        InfoMessages.Add(new InfoMessageClass()
        {
            Id = info_id,
            InfoMessage = message,
            Color = GetHexCodeByType(type)
        });
        AutoDeleteInfoMessage(info_id);
    }

    private async void AutoDeleteInfoMessage(int infoMessageId)
    {
        try
        {
            await Task.Delay(3000);
            InfoMessages.Remove(_infoMessages.First(item => item.Id == infoMessageId));
        }
        catch
        {
            //
        }
    }

    private string GetLaunchPath()
    {
        FileInfo path = new FileInfo(_launchPath);

        if (!path.Exists)
            return "";

        return path.Directory!.ToString();
    }

    #region GetSet

    private string _replaceAnnouncementAnswer = "none";

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

    private string _oldAnnouncementImage;

    public string OldAnnouncementImage
    {
        get => _oldAnnouncementImage;
        set
        {
            _oldAnnouncementImage = value;
            OnPropertyChanged();
        }
    }

    private string _newAnnouncementImage;

    public string NewAnnouncementImage
    {
        get => _newAnnouncementImage;
        set
        {
            _newAnnouncementImage = value;
            OnPropertyChanged();
        }
    }

    private string _replaceFileName;

    public string ReplaceFileName
    {
        get => _replaceFileName;
        set
        {
            _replaceFileName = value;
            OnPropertyChanged();
        }
    }

    private string _replaceNewFileName;

    public string ReplaceNewFileName
    {
        get => _replaceNewFileName;
        set
        {
            _replaceNewFileName = value;
            OnPropertyChanged();
        }
    }

    private string _hexCodeColor = "#000000";

    public string HexCodeColor
    {
        get => _hexCodeColor;
        set
        {
            _hexCodeColor = GetHexCode(value);
            OnPropertyChanged();
        }
    }

    private bool _isReplaceAnnouncementOpened;

    public bool IsReplaceAnnouncementOpened
    {
        get => _isReplaceAnnouncementOpened;
        set
        {
            _isReplaceAnnouncementOpened = value;
            OnPropertyChanged();
        }
    }

    private Object _selectedDisplay;

    public Object SelectedDisplay
    {
        get => _selectedDisplay;
        set
        {
            _selectedDisplay = value;
            OnPropertyChanged();
            SelectedDisplayContent = ((DisplayClass)_selectedDisplay).DisplayNum.ToString();
        }
    }

    private Object _selectedAnnouncement;

    public Object SelectedAnnouncement
    {
        get => _selectedAnnouncement;
        set
        {
            _selectedAnnouncement = value;
            OnPropertyChanged();
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

    private ObservableCollection<InfoMessageClass> _infoMessages = new();

    public ObservableCollection<InfoMessageClass> InfoMessages
    {
        get => _infoMessages;
        set
        {
            _infoMessages = value;
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

    private bool _isBlackout;

    public bool IsBlackout
    {
        get => _isBlackout;
        set
        {
            _isBlackout = value;
            OnPropertyChanged();
        }
    }

    private string _activityStart = "09:00:00";

    public string ActivityStart
    {
        get => _activityStart;
        set
        {
            _activityStart = value;
            OnPropertyChanged();
        }
    }

    private string _activityEnd = "21:00:00";

    public string ActivityEnd
    {
        get => _activityEnd;
        set
        {
            _activityEnd = value;
            OnPropertyChanged();
        }
    }

    private string _blackoutStart = "22:00:00";

    public string BlackoutStart
    {
        get => _blackoutStart;
        set
        {
            _blackoutStart = value;
            OnPropertyChanged();
        }
    }

    private string _blackoutEnd = "6:00:00";

    public string BlackoutEnd
    {
        get => _blackoutEnd;
        set
        {
            _blackoutEnd = value;
            OnPropertyChanged();
        }
    }

    private bool _launchAutoDeleteFile;

    public bool LaunchAutoDeleteFile
    {
        get => _launchAutoDeleteFile;
        set
        {
            _launchAutoDeleteFile = value;
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

    private string _selectedDisplayContent;

    public string SelectedDisplayContent
    {
        get => _selectedDisplayContent;
        set
        {
            _selectedDisplayContent = value;
            OnPropertyChanged();
        }
    }

    #endregion

    private void OnPropertyChanged([CallerMemberName] string property = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public class DisplayClass
    {
        public int DisplayNum { get; set; }
        public string? DirectoryPath { get; set; } = "";
        public int? Rate { get; set; } = 60;
        public bool? IsModeTwo { get; set; } = true;
    }

    public class InfoMessageClass
    {
        public int Id { get; set; }
        public string InfoMessage { get; set; }
        public string Color { get; set; }
    }

    public class AnnouncementClass
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime? ActualStart { get; set; } = DateTime.Now;
        public DateTime? ActualEnd { get; set; } = DateTime.MaxValue;
    }

    class ActionClass
    {
        public string Action { get; set; } = "";
        public string?[] Args { get; set; } = { };
        public DateTime ActionTime = DateTime.Now;
    }
}