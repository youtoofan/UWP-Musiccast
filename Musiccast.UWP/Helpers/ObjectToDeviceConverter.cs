using Musiccast.Models;
using System;
using Windows.UI.Xaml.Data;

namespace Musiccast.Helpers
{
    public class ObjectToDeviceConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as Device;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
