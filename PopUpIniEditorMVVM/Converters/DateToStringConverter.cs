using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace PopUpIniEditorMVVM;

public class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string val = value.ToString();
        
        if (!DateTime.TryParse(val, out var dt))
            return new BindingNotification(new Exception("Дата заполнена неверно. Данное значение сохранено не будет"),BindingErrorType.DataValidationError);

        return dt;
    }
    
}