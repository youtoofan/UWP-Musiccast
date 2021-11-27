using System;
using Windows.UI.Xaml.Data;

namespace Musiccast.Helpers
{
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0.25;

            var isTrue = (bool)value;
            return isTrue ? 0.9: 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
