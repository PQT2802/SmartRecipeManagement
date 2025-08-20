using SmartRecipe.Domain.Entities;

namespace SmartRecipe.Wpf.Services
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        Task LogoutAsync();
        bool IsAuthenticated { get; }
        User CurrentUser { get; }
        event Action<User> UserChanged;
    }
}