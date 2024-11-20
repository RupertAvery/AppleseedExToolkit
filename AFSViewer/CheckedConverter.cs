using System.Globalization;
using System.Windows.Data;

namespace AFSViewer;

public class CheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (string)value == (string)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (string)parameter;
    }
}