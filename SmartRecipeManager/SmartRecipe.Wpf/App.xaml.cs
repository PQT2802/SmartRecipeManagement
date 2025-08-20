using Firebase.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartRecipe.Infrastructure.DB;
using SmartRecipe.Infrastructure.Repo.Comment;
using SmartRecipe.Infrastructure.Repo.Generic;
using SmartRecipe.Infrastructure.Repo.Ingredient;
using SmartRecipe.Infrastructure.Repo.Like;
using SmartRecipe.Infrastructure.Repo.MediaFile;
using SmartRecipe.Infrastructure.Repo.NutritionInfo;
using SmartRecipe.Infrastructure.Repo.Recipe;
using SmartRecipe.Infrastructure.Repo.Step;
using SmartRecipe.Infrastructure.Repo.User;
using SmartRecipe.Service.Interface;
using SmartRecipe.Service.Service;
using SmartRecipe.Service.Service.Imple;
using SmartRecipe.Service.Service.Interface;
using SmartRecipe.Service.Service.UnitOfWork;
using SmartRecipe.Wpf.Services;
using SmartRecipe.Wpf.ViewModels;
using SmartRecipe.Wpf.Views;
using System.IO;
using System.Windows;

namespace SmartRecipe.Wpf
{
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; }
        public static IHost AppHost { get; private set; }

        public App()
        {
            try
            {
                // Load configuration first
                Configuration = LoadConfiguration();

                // Create host with DI and config
                AppHost = CreateHostBuilder().Build();
            }
            catch (Exception ex)
            {
                ShowStartupError("Configuration Error", ex);
                Current?.Shutdown();
            }
        }

        private static IConfiguration LoadConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

                return builder.Build();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    try
                    {
                        ConfigureServices(services);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to configure services: {ex.Message}", ex);
                    }
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddSingleton(Configuration);

            // HttpClient for Spoonacular API
            services.AddHttpClient<ISpoonacularService, SpoonacularService>();

            // Database Configuration
            ConfigureDatabaseServices(services);

            // Firebase Configuration with better error handling
            ConfigureFirebaseServices(services);

            // Repository Configuration
            ConfigureRepositories(services);

            // Business Services Configuration
            ConfigureBusinessServices(services);

            // WPF Services Configuration
            ConfigureWpfServices(services);

            // ViewModels Configuration
            ConfigureViewModels(services);

            // Views Configuration
            ConfigureViews(services);
        }

        private static void ConfigureDatabaseServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=.;Initial Catalog=SmartRecipeManager;Persist Security Info=True;User ID=sa;Password=12345;Encrypt=false";
            }

            services.AddDbContext<SmartRecipeDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging(false);
                options.EnableServiceProviderCaching();
            });
        }

        private static void ConfigureFirebaseServices(IServiceCollection services)
        {
            try
            {
                var firebaseSection = Configuration.GetSection("Firebase");
                var storageBucket = firebaseSection["StorageBucket"];

                if (string.IsNullOrEmpty(storageBucket))
                {
                    // Use a fallback configuration for development
                    storageBucket = "lmsproject-5a473.appspot.com";
                    System.Diagnostics.Debug.WriteLine($"Warning: Using fallback Firebase bucket: {storageBucket}");
                }

                // Validate bucket format
                if (!storageBucket.EndsWith(".appspot.com"))
                {
                    throw new InvalidOperationException($"Invalid Firebase storage bucket format: {storageBucket}");
                }

                // Register Firebase services with error handling
                services.AddSingleton<FirebaseStorage>(provider =>
                {
                    try
                    {
                        return new FirebaseStorage(storageBucket);
                    }
                    catch (Exception ex)
                    {
                        var logger = provider.GetService<ILogger<App>>();
                        logger?.LogError(ex, "Failed to initialize FirebaseStorage with bucket: {Bucket}", storageBucket);

                        // Return a null object that can be handled gracefully
                        return null;
                    }
                });

                services.AddTransient<IFirebaseService>(provider =>
                {
                    try
                    {
                        return new FirebaseService(storageBucket);
                    }
                    catch (Exception ex)
                    {
                        var logger = provider.GetService<ILogger<App>>();
                        logger?.LogError(ex, "Failed to initialize FirebaseService with bucket: {Bucket}", storageBucket);

                        // Return a mock service for development
                        return new MockFirebaseService();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase configuration error: {ex.Message}");

                // Register mock services as fallback
                services.AddSingleton<FirebaseStorage>(provider => null);
                services.AddTransient<IFirebaseService, MockFirebaseService>();
            }
        }

        private static void ConfigureRepositories(IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IMediaFileRepository, MediaFileRepository>();
            services.AddScoped<INutritionInfoRepository, NutritionInfoRepository>();
            services.AddScoped<IStepRepository, StepRepository>();
        }

        private static void ConfigureBusinessServices(IServiceCollection services)
        {
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IMediaFileService, MediaFileService>();
            services.AddScoped<ISpoonacularService, SpoonacularService>();
            services.AddScoped<IEnhancedRecipeService, EnhancedRecipeService>();
        }

        private static void ConfigureWpfServices(IServiceCollection services)
        {
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
        }

        private static void ConfigureViewModels(IServiceCollection services)
        {
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<RecipeListViewModel>();
            services.AddTransient<EnhancedRecipeListViewModel>();
            services.AddTransient<RecipeDetailViewModel>();
            services.AddTransient<CreateRecipeViewModel>();
            services.AddTransient<UserProfileViewModel>();
            services.AddTransient<SpoonacularRecipeDetailViewModel>();
        }

        private static void ConfigureViews(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginView>();
            services.AddTransient<EnhancedRecipeListView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<RecipeListView>();
            services.AddTransient<RecipeDetailView>();
            services.AddTransient<CreateRecipeView>();
            services.AddTransient<UserProfileView>();
            services.AddTransient<SpoonacularRecipeDetailView>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await AppHost.StartAsync();

                // Initialize database
                await InitializeDatabaseAsync();

                // Create and show MainWindow using DI
                var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                ShowStartupError("Startup Error", ex);
                Current?.Shutdown();
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                using var scope = AppHost.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartRecipeDbContext>();

                // Ensure database is created
                await dbContext.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                var logger = AppHost.Services.GetService<ILogger<App>>();
                logger?.LogError(ex, "Failed to initialize database");
                // Don't throw - let the app continue with potential database issues
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                if (AppHost != null)
                {
                    await AppHost.StopAsync(TimeSpan.FromSeconds(5));
                    AppHost.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
            }
            finally
            {
                base.OnExit(e);
            }
        }

        private static void ShowStartupError(string title, Exception ex)
        {
            var message = $"{ex.Message}\n\nInner Exception: {ex.InnerException?.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Mock Firebase Service for development/fallback
    public class MockFirebaseService : IFirebaseService
    {
        public Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "images")
        {
            // Return a mock URL
            return Task.FromResult($"https://mock-storage.com/{folder}/{Guid.NewGuid()}_{fileName}");
        }

        public Task<string> UploadVideoAsync(byte[] videoData, string fileName, string folder = "videos")
        {
            // Return a mock URL
            return Task.FromResult($"https://mock-storage.com/{folder}/{Guid.NewGuid()}_{fileName}");
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            // Always return success for mock
            return Task.FromResult(true);
        }
    }
}