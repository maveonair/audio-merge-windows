using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioMerge.App.Converters
{
    internal class BooleanInverseConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
        {
            return !(bool?)value ?? true;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value as bool?);
        }
    }
}
