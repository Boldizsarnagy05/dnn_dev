using System.Collections.Generic;

namespace Teszko.ReceptModulRecept_modul.Models
{
    public sealed class RecipeListViewModel
    {
        public string ActiveMealType { get; set; }
        public List<RecipeCardViewModel> Recipes { get; set; } = new List<RecipeCardViewModel>();
    }

    public sealed class RecipeCardViewModel
    {
        public string CategoryBvin { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string MealType { get; set; }
        public int Servings { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int TotalCalories { get; set; }
    }

    public sealed class RecipeDetailViewModel
    {
        public string CategoryBvin { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MealType { get; set; }
        public int Servings { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int TotalCalories { get; set; }
        public decimal EstimatedCost { get; set; }
        public List<RecipeIngredientViewModel> Ingredients { get; set; } = new List<RecipeIngredientViewModel>();
        public List<string> Steps { get; set; } = new List<string>();
    }

    public sealed class RecipeIngredientViewModel
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

    public sealed class AddRecipeCartRequest
    {
        public string CategoryBvin { get; set; }
        public int Servings { get; set; }
        public List<CartLineRequest> Items { get; set; } = new List<CartLineRequest>();
    }

    public sealed class CartLineRequest
    {
        public string ProductBvin { get; set; }
        public int Quantity { get; set; }
    }

    public sealed class CartActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
