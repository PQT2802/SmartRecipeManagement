using System.Windows.Controls;

namespace SmartRecipe.Wpf.Services
{
    public interface INavigationService
    {
        bool CanNavigateBack { get; }
        event Action<UserControl> Navigated;

        void NavigateTo<T>() where T : UserControl;
        void NavigateTo<T>(object parameter) where T : UserControl;
        void NavigateBack();
    }
}