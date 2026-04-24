using System.Collections.Generic;

namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class SaveRecipeRequest
    {
        public int? RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Steps { get; set; }
        public string Tags { get; set; }
        public int Servings { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int? TotalCalories { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string AuthorName { get; set; }
        public string PreviewImageUrl { get; set; }
        public string Status { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin { get; set; }
        public bool CreateOrUpdateBundle { get; set; }
        public bool PublishAfterSave { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
    }
}
