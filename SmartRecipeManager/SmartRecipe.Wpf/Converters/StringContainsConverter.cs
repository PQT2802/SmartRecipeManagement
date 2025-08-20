using System.Globalization;
using System.Windows.Data;

namespace SmartRecipe.Wpf.Converters
{
    public class StringContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && parameter is string searchText)
            {
                return text.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}