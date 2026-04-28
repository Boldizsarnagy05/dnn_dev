namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class RevokeRecipeRequest
    {
        public int? RecipeId { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin { get; set; }
    }
}
