using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;
using SmartRecipe.Service.Service.Interface;
using System.Security.Cryptography;
using System.Text;

namespace SmartRecipe.Wpf.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private User _currentUser;

        public AuthenticationService(IUserService userService)
        {
            _userService = userService;
        }

        public bool IsAuthenticated => _currentUser != null;
        public User CurrentUser => _currentUser;

        public event Action<User> UserChanged;

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                // This is a simplified approach - in real apps, use proper authentication
                var user = await _userService.GetUserByEmailAsync(email);
                
                if (user != null && user.PasswordHash == password)
                {
                    _currentUser = user;
                    UserChanged?.Invoke(_currentUser);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = username,
                    Email = email,
                    PasswordHash = password,
                    Role = UserRole.Viewer
                };

                await _userService.CreateUserAsync(user);
                _currentUser = user;
                UserChanged?.Invoke(_currentUser);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            UserChanged?.Invoke(null);
            await Task.CompletedTask;
        }

    }
}