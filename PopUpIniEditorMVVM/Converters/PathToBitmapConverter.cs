using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace PopUpIniEditorMVVM;

public class PathToBitmapConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return new Exception("value is null");
        
        string path = (string)value;

        FileInfo file = new FileInfo(path);
        
        if(file is null)
            return new Exception("file is null");
        
        if(!file.Exists)
            return new Exception("file isnt exists");
        
        return new Bitmap(path);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}