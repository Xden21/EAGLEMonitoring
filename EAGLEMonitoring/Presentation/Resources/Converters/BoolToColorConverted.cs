using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EAGLEMonitoring.Presentation.Resources.Converters
{
    public class BoolToColorConverted : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolval = (bool)value;
            return boolval ? System.Windows.Media.Brushes.LimeGreen: System.Windows.Media.Brushes.OrangeRed ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
