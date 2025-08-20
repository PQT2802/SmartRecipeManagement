using System.Globalization;
using System.Windows.Data;

namespace SmartRecipe.Wpf.Converters
{
    public class CleanDescriptionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string description)
            {
                // Remove [External] and [Discover] tags from description
                return description
                    .Replace("[External] ", "")
                    .Replace("[Discover] ", "")
                    .Trim();
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}