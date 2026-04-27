using System.Collections.Generic;

namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class RecipeMetadata
    {
        public string MealType { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string Tags { get; set; }
        public int Servings { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int TotalCalories { get; set; }
        public decimal EstimatedCost { get; set; }
        public string PreviewImageUrl { get; set; }
        public List<RecipeIngredientMetadata> Ingredients { get; set; } = new List<RecipeIngredientMetadata>();
    }

    public sealed class RecipeIngredientMetadata
    {
        public string ProductBvin { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public int Calories { get; set; }
        public decimal Price { get; set; }
        public decimal PackageQuantity { get; set; }
        public string PackageUnit { get; set; }
        public int SortOrder { get; set; }
    }
}
