using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace SmartRecipe.Wpf.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<UserControl> _navigationStack = new();

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool CanNavigateBack => _navigationStack.Count > 1;

        public event Action<UserControl> Navigated;
        public event Action<object> CurrentViewChanged;

        public void NavigateTo<T>() where T : UserControl
        {
            // Create a new instance for the view
            var view = _serviceProvider.GetRequiredService<T>();

            // Push to navigation stack
            _navigationStack.Push(view);

            // Notify after adding to stack
            Navigated?.Invoke(view);
            CurrentViewChanged?.Invoke(view);
        }

        public void NavigateTo<T>(object parameter) where T : UserControl
        {
            // Create a new instance for the view
            var view = _serviceProvider.GetRequiredService<T>();

            // If view implements INavigationAware, pass parameter
            if (view.DataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            // Push to navigation stack
            _navigationStack.Push(view);

            // Notify after adding to stack
            Navigated?.Invoke(view);
            CurrentViewChanged?.Invoke(view);
        }

        public void NavigateBack()
        {
            if (!CanNavigateBack) return;

            // Pop current view
            _navigationStack.Pop();

            // Get previous view
            var previousView = _navigationStack.Peek();

            // Call OnNavigatedTo with null to refresh
            if (previousView.DataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(null);
            }

            // Notify navigation
            Navigated?.Invoke(previousView);
            CurrentViewChanged?.Invoke(previousView);
        }
    }


    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
    }
}