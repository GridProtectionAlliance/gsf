using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace TimeSeriesFramework.UI.Converters
{

    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string temp = value.ToString().ToUpper();
                if (temp == "TRUE" || temp == "1" || temp == "IPHA" || temp == "IPHM" || temp == "VPHA" || temp == "VPHM" || temp == "FREQ")
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class IntegerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility;
            if (value != null)
            {
                int temp;
                if (Int32.TryParse(value.ToString(), out temp))
                {
                    if (temp > 0)
                        visibility = Visibility.Visible;
                    else
                        visibility = Visibility.Collapsed;
                }
                else
                    visibility = Visibility.Collapsed;
            }
            else
                visibility = Visibility.Collapsed;

            //This was added for Browse->Devices screen where ParentID IS NULL then Update Configuration link is visible.
            //Just pass in bool parameter to invert boolean value.
            if (parameter is bool && (bool)parameter)
                return visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
