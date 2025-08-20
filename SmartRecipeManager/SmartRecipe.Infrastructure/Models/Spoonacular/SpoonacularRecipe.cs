using Newtonsoft.Json;

namespace SmartRecipe.Infrastructure.Models.Spoonacular
{
    public class SpoonacularRecipe
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;

        [JsonProperty("readyInMinutes")]
        public int ReadyInMinutes { get; set; }

        [JsonProperty("servings")]
        public int Servings { get; set; }

        [JsonProperty("sourceUrl")]
        public string SourceUrl { get; set; } = string.Empty;

        [JsonProperty("spoonacularSourceUrl")]
        public string SpoonacularSourceUrl { get; set; } = string.Empty;

        [JsonProperty("healthScore")]
        public double? HealthScore { get; set; }

        [JsonProperty("spoonacularScore")]
        public double? SpoonacularScore { get; set; }

        [JsonProperty("pricePerServing")]
        public double? PricePerServing { get; set; }

        [JsonProperty("analyzedInstructions")]
        public List<AnalyzedInstruction> AnalyzedInstructions { get; set; } = new();

        [JsonProperty("extendedIngredients")]
        public List<ExtendedIngredient> ExtendedIngredients { get; set; } = new();

        [JsonProperty("nutrition")]
        public SpoonacularNutrition? Nutrition { get; set; }

        [JsonProperty("cuisines")]
        public List<string> Cuisines { get; set; } = new();

        [JsonProperty("dishTypes")]
        public List<string> DishTypes { get; set; } = new();

        [JsonProperty("diets")]
        public List<string> Diets { get; set; } = new();

        [JsonProperty("occasions")]
        public List<string> Occasions { get; set; } = new();
    }

    public class AnalyzedInstruction
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("steps")]
        public List<InstructionStep> Steps { get; set; } = new();
    }

    public class InstructionStep
    {
        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("step")]
        public string Step { get; set; } = string.Empty;

        [JsonProperty("ingredients")]
        public List<StepIngredient> Ingredients { get; set; } = new();

        [JsonProperty("equipment")]
        public List<StepEquipment> Equipment { get; set; } = new();
    }

    public class StepIngredient
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("localizedName")]
        public string LocalizedName { get; set; } = string.Empty;

        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;
    }

    public class StepEquipment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("localizedName")]
        public string LocalizedName { get; set; } = string.Empty;

        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;
    }

    public class ExtendedIngredient
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("aisle")]
        public string Aisle { get; set; } = string.Empty;

        [JsonProperty("image")]
        public string Image { get; set; } = string.Empty;

        [JsonProperty("consistency")]
        public string Consistency { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("nameClean")]
        public string NameClean { get; set; } = string.Empty;

        [JsonProperty("original")]
        public string Original { get; set; } = string.Empty;

        [JsonProperty("originalName")]
        public string OriginalName { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonProperty("meta")]
        public List<string> Meta { get; set; } = new();

        [JsonProperty("measures")]
        public Measures Measures { get; set; } = new();
    }

    public class Measures
    {
        [JsonProperty("us")]
        public Measurement Us { get; set; } = new();

        [JsonProperty("metric")]
        public Measurement Metric { get; set; } = new();
    }

    public class Measurement
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unitShort")]
        public string UnitShort { get; set; } = string.Empty;

        [JsonProperty("unitLong")]
        public string UnitLong { get; set; } = string.Empty;
    }

    public class SpoonacularNutrition
    {
        [JsonProperty("nutrients")]
        public List<Nutrient> Nutrients { get; set; } = new();

        [JsonProperty("properties")]
        public List<NutritionProperty> Properties { get; set; } = new();

        [JsonProperty("flavonoids")]
        public List<Flavonoid> Flavonoids { get; set; } = new();

        [JsonProperty("caloricBreakdown")]
        public CaloricBreakdown CaloricBreakdown { get; set; } = new();

        [JsonProperty("weightPerServing")]
        public WeightPerServing WeightPerServing { get; set; } = new();
    }

    public class Nutrient
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonProperty("percentOfDailyNeeds")]
        public double? PercentOfDailyNeeds { get; set; }
    }

    public class NutritionProperty
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;
    }

    public class Flavonoid
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;
    }

    public class CaloricBreakdown
    {
        [JsonProperty("percentProtein")]
        public double PercentProtein { get; set; }

        [JsonProperty("percentFat")]
        public double PercentFat { get; set; }

        [JsonProperty("percentCarbs")]
        public double PercentCarbs { get; set; }
    }

    public class WeightPerServing
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;
    }
}