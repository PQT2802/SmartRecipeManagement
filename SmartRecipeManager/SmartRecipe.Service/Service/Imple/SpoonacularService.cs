using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartRecipe.Domain.Entities;
using SmartRecipe.Domain.Enum;
using SmartRecipe.Infrastructure.Models.Spoonacular;
using SmartRecipe.Service.Interface;
using System.Text.RegularExpressions;

namespace SmartRecipe.Service.Service.Imple
{
    public class SpoonacularService : ISpoonacularService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SpoonacularService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly int _rateLimitPerMinute;
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly SemaphoreSlim _rateLock = new(1, 1);

        public SpoonacularService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SpoonacularService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _apiKey = "5363d770e41e4c109e7e4cd5b8780369";
            //_apiKey = _configuration["Spoonacular:ApiKey"] ?? throw new InvalidOperationException("Spoonacular API key not found in configuration");
            _baseUrl = _configuration["Spoonacular:BaseUrl"] ?? "https://api.spoonacular.com";
            _rateLimitPerMinute = int.Parse(_configuration["Spoonacular:RateLimitPerMinute"] ?? "1");

            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SmartRecipe/1.0");
        }

        private async Task EnsureRateLimitAsync(CancellationToken cancellationToken = default)
        {
            // Early check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            var currentTime = DateTime.UtcNow;
            var timeSinceLastRequest = currentTime - _lastRequestTime;
            var minInterval = TimeSpan.FromSeconds(60.0 / _rateLimitPerMinute);

            if (timeSinceLastRequest < minInterval)
            {
                var initialDelay = minInterval - timeSinceLastRequest;
                _logger.LogInformation("Rate limiting: pre-lock waiting {Delay}ms", initialDelay.TotalMilliseconds);

                try
                {
                    // Wait without holding the lock
                    await Task.Delay(initialDelay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Rate limiting delay was cancelled");
                    throw; // Re-throw to maintain cancellation semantics
                }
            }

            // Early check for cancellation before acquiring lock
            cancellationToken.ThrowIfCancellationRequested();

            // Now acquire the lock to update the last request time
            await _rateLock.WaitAsync(cancellationToken);
            try
            {
                // Final check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Double-check if we still need to wait
                timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                if (timeSinceLastRequest < minInterval)
                {
                    var remainingDelay = minInterval - timeSinceLastRequest;
                    _logger.LogInformation("Rate limiting: additional waiting {Delay}ms", remainingDelay.TotalMilliseconds);

                    try
                    {
                        await Task.Delay(remainingDelay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Rate limiting additional delay was cancelled");
                        throw;
                    }
                }

                // Update the last request time
                _lastRequestTime = DateTime.UtcNow;
            }
            finally
            {
                _rateLock.Release();
            }
        }

        private async Task<T?> MakeApiRequestAsync<T>(string endpoint, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                // await EnsureRateLimitAsync(cancellationToken);

                var separator = endpoint.Contains("?") ? "&" : "?";
                var url = $"{endpoint}{separator}apiKey={_apiKey}";
                _logger.LogInformation("Making Spoonacular API request to: {Url}", url.Replace(_apiKey, "***"));

                // Pass the cancellation token to HttpClient methods
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Spoonacular API request failed with status {StatusCode}: {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<T>(content);

                _logger.LogInformation("Successfully received Spoonacular API response");
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("API request to {Endpoint} was cancelled", endpoint);
                throw; // Rethrow to allow proper cancellation handling
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making Spoonacular API request to {Endpoint}", endpoint);
                return null;
            }
        }

        public async Task<SpoonacularSearchResponse> SearchRecipesAsync(
            string query,
            int number = 10,
            int offset = 0,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/recipes/complexSearch" +
                          $"?query={Uri.EscapeDataString(query)}" +
                          $"&number={number}" +
                          $"&offset={offset}" +
                          $"&addRecipeInformation=true" +
                          $"&fillIngredients=true";

            var result = await MakeApiRequestAsync<SpoonacularSearchResponse>(endpoint, cancellationToken);
            return result ?? new SpoonacularSearchResponse();
        }

        public async Task<SpoonacularRecipe?> GetRecipeInformationAsync(
            int id,
            bool includeNutrition = true,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/recipes/{id}/information" +
                          $"?includeNutrition={includeNutrition.ToString().ToLower()}";

            return await MakeApiRequestAsync<SpoonacularRecipe>(endpoint, cancellationToken);
        }

        public async Task<List<SpoonacularRecipe>> GetRandomRecipesAsync(
            int number = 1,
            string? tags = null,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/recipes/random" +
                          $"?number={number}";

            if (!string.IsNullOrEmpty(tags))
            {
                endpoint += $"&tags={Uri.EscapeDataString(tags)}";
            }

            var response = await MakeApiRequestAsync<RandomRecipesResponse>(endpoint, cancellationToken);
            return response?.Recipes ?? new List<SpoonacularRecipe>();
        }

        public async Task<SpoonacularSearchResponse> SearchRecipesByIngredientsAsync(
            string ingredients,
            int number = 10,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/recipes/findByIngredients" +
                          $"?ingredients={Uri.EscapeDataString(ingredients)}" +
                          $"&number={number}" +
                          $"&ranking=1" +
                          $"&ignorePantry=true";

            var result = await MakeApiRequestAsync<SpoonacularSearchResponse>(endpoint, cancellationToken);
            return result ?? new SpoonacularSearchResponse();
        }

        public async Task<List<SpoonacularRecipeSimple>> GetSimilarRecipesAsync(
            int id,
            int number = 3,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/recipes/{id}/similar" +
                          $"?number={number}";

            var result = await MakeApiRequestAsync<List<SpoonacularRecipeSimple>>(endpoint, cancellationToken);
            return result ?? new List<SpoonacularRecipeSimple>();
        }

        public async Task<SpoonacularNutrition?> GetRecipeNutritionAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/recipes/{id}/nutritionWidget.json";
            return await MakeApiRequestAsync<SpoonacularNutrition>(endpoint, cancellationToken);
        }

        public async Task<Recipe> ConvertToLocalRecipeAsync(
            SpoonacularRecipe spoonacularRecipe,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // This method doesn't make API calls, so we don't need to use the cancellation token
            // But we include it for interface compliance

            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Title = CleanHtml(spoonacularRecipe.Title),
                Description = CleanHtml(spoonacularRecipe.Summary),
                Category = DetermineCategory(spoonacularRecipe.DishTypes),
                PrepTimeMinutes = Math.Max(5, spoonacularRecipe.ReadyInMinutes / 2),
                CookTimeMinutes = Math.Max(5, spoonacularRecipe.ReadyInMinutes / 2),
                Servings = spoonacularRecipe.Servings > 0 ? spoonacularRecipe.Servings : 4,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                Ingredients = new List<Ingredient>(),
                Steps = new List<Step>(),
                MediaFiles = new List<MediaFile>()
            };

            // Convert ingredients
            foreach (var ingredient in spoonacularRecipe.ExtendedIngredients ?? Enumerable.Empty<ExtendedIngredient>())
            {
                recipe.Ingredients.Add(new Ingredient
                {
                    Id = Guid.NewGuid(),
                    Name = ingredient.Name,
                    Quantity = $"{ingredient.Amount} {ingredient.Unit}".Trim(),
                    RecipeId = recipe.Id
                });
            }

            // Convert instructions
            var stepOrder = 1;
            foreach (var instruction in spoonacularRecipe.AnalyzedInstructions ?? Enumerable.Empty<AnalyzedInstruction>())
            {
                foreach (var step in instruction.Steps ?? Enumerable.Empty<InstructionStep>())
                {
                    recipe.Steps.Add(new Step
                    {
                        Id = Guid.NewGuid(),
                        Description = CleanHtml(step.Step),
                        Order = stepOrder++,
                        RecipeId = recipe.Id
                    });
                }
            }

            // Add nutrition info if available
            if (spoonacularRecipe.Nutrition?.Nutrients != null)
            {
                var calories = spoonacularRecipe.Nutrition.Nutrients.FirstOrDefault(n => n.Name.Equals("Calories", StringComparison.OrdinalIgnoreCase));
                var protein = spoonacularRecipe.Nutrition.Nutrients.FirstOrDefault(n => n.Name.Equals("Protein", StringComparison.OrdinalIgnoreCase));
                var fat = spoonacularRecipe.Nutrition.Nutrients.FirstOrDefault(n => n.Name.Equals("Fat", StringComparison.OrdinalIgnoreCase));
                var carbs = spoonacularRecipe.Nutrition.Nutrients.FirstOrDefault(n => n.Name.Equals("Carbohydrates", StringComparison.OrdinalIgnoreCase));

                recipe.NutritionInfo = new NutritionInfo
                {
                    Id = Guid.NewGuid(),
                    Calories = (decimal)(calories?.Amount ?? 0),
                    Protein = (decimal)(protein?.Amount ?? 0),
                    Fat = (decimal)(fat?.Amount ?? 0),
                    Carbs = (decimal)(carbs?.Amount ?? 0),
                    RecipeId = recipe.Id
                };
            }

            // Add recipe image as media file if available
            if (!string.IsNullOrEmpty(spoonacularRecipe.Image))
            {
                recipe.MediaFiles.Add(new MediaFile
                {
                    Id = Guid.NewGuid(),
                    FileName = $"spoonacular_recipe_{spoonacularRecipe.Id}.jpg",
                    FileUrl = spoonacularRecipe.Image,
                    FileType = MediaType.Image,
                    UploadedAt = DateTime.UtcNow,
                    RecipeId = recipe.Id
                });
            }

            return recipe;
        }

        private static string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return Regex.Replace(html, "<.*?>", string.Empty).Trim();
        }

        private static RecipeCategory DetermineCategory(List<string> dishTypes)
        {
            if (dishTypes == null || !dishTypes.Any())
                return RecipeCategory.MainCourse;

            var dishType = dishTypes.FirstOrDefault()?.ToLower() ?? "";

            return dishType switch
            {
                var x when x.Contains("appetizer") || x.Contains("starter") => RecipeCategory.Appetizer,
                var x when x.Contains("breakfast") => RecipeCategory.Breakfast,
                var x when x.Contains("lunch") || x.Contains("main") || x.Contains("dinner") => RecipeCategory.MainCourse,
                var x when x.Contains("dessert") || x.Contains("sweet") => RecipeCategory.Dessert,
                var x when x.Contains("drink") || x.Contains("beverage") => RecipeCategory.Beverage,
                var x when x.Contains("snack") => RecipeCategory.Snack,
                var x when x.Contains("side") => RecipeCategory.SideDish,
                _ => RecipeCategory.MainCourse
            };
        }
    }

    // Helper class for random recipes response
    public class RandomRecipesResponse
    {
        [JsonProperty("recipes")]
        public List<SpoonacularRecipe> Recipes { get; set; } = new();
    }
}
