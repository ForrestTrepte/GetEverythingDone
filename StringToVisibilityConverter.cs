using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GetEverythingDone
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
                return Visibility.Collapsed; // Hide when text exists
            return Visibility.Visible; // Show when text is empty
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
