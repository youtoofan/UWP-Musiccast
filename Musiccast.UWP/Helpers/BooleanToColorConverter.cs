using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Musiccast.Helpers
{
    public class BooleanToColorConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return new SolidColorBrush(Colors.LightGray);

            var isTrue = (bool)value;
            return isTrue ? new SolidColorBrush(Colors.OrangeRed): new SolidColorBrush(Colors.SlateGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
