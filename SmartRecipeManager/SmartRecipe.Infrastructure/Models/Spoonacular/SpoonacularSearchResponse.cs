using Newtonsoft.Json;

namespace SmartRecipe.Infrastructure.Models.Spoonacular
{
    public class SpoonacularSearchResponse
    {
        [JsonProperty("results")]
        public List<SpoonacularRecipeSimple> Results { get; set; } = new();

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
    }

    public class SpoonacularRecipeSimple
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;

        [JsonProperty("imageType")]
        public string ImageType { get; set; } = string.Empty;

        [JsonProperty("nutrition")]
        public SpoonacularNutritionSimple? Nutrition { get; set; }
    }

    public class SpoonacularNutritionSimple
    {
        [JsonProperty("nutrients")]
        public List<Nutrient> Nutrients { get; set; } = new();
    }
}