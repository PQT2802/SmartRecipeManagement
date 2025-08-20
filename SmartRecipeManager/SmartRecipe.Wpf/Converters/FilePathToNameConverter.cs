using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace SmartRecipe.Wpf.Converters
{
    public class FilePathToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath && !string.IsNullOrWhiteSpace(filePath))
                return Path.GetFileName(filePath);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
