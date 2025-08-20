using System.Globalization;
using System.Windows.Data;

namespace SmartRecipe.Wpf.Converters
{
    public class LikeToEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLiked)
                return isLiked ? "❤️" : "🤍";
            return "🤍";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
