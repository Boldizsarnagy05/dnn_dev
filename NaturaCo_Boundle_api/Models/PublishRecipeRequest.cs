namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class PublishRecipeRequest
    {
        public int? RecipeId { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin { get; set; }
    }
}
