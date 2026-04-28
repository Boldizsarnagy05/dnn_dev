using System.Collections.Generic;
using NaturaCo.RecipeSyncApi.Models;

namespace NaturaCo.RecipeSyncApi.Services
{
    public interface IHotcakesRecipeGateway
    {
        IReadOnlyCollection<RecipeListItemDto> ListRecipes();
        SaveRecipeRequest LoadRecipe(int recipeId);
        CategorySyncReference UpsertRecipeCategory(SaveRecipeRequest request);
        void ReplaceCategoryProducts(string categoryBvin, IReadOnlyCollection<RecipeIngredientDto> ingredients);
        BundleSyncReference UpsertBundle(SaveRecipeRequest request, string categoryBvin);
        void Publish(string categoryBvin, string bundleBvin);
        void Revoke(string categoryBvin, string bundleBvin);
    }

    public sealed class CategorySyncReference
    {
        public string CategoryBvin { get; set; }
    }

    public sealed class BundleSyncReference
    {
        public string BundleBvin { get; set; }
    }
}
