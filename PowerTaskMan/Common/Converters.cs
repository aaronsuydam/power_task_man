using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;

namespace PowerTaskMan.Common
{
   


    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value; // Not used, so we can leave this unchanged
        }
    }
}
