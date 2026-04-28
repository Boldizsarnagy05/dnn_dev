namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class RecipeListItemDto
    {
        public int? RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin { get; set; }
        public string MealType { get; set; }
        public string Status { get; set; }
        public string ShortDescription { get; set; }
        public int Servings { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int TotalCalories { get; set; }
    }
}
