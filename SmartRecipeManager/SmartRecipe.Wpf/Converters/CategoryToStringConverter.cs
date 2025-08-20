using SmartRecipe.Domain.Enum;
using System.Globalization;
using System.Windows.Data;

namespace SmartRecipe.Wpf.Converters
{
    public class CategoryToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RecipeCategory category)
                return category.ToString();
            if (value == null)
                return "All Categories";
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == "All Categories")
                return null;
            if (Enum.TryParse<RecipeCategory>(value?.ToString(), out var category))
                return category;
            return null;
        }
    }
}
