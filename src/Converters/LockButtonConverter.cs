using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaContacts.Converters;

public class LockButtonConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool locked && locked ? "🔒 Locked" : "🔓 Unlocked";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}