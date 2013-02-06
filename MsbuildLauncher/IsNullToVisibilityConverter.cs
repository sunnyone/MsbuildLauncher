using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace MsbuildLauncher
{
    class IsNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((string)parameter) == "VisibleIfNull")
            {
                return (value == null) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (((string)parameter) == "CollapsedIfNull")
            {
                return (value == null) ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
