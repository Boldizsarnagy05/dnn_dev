using NaturaCo.RecipeSyncApi.Models;

namespace NaturaCo.RecipeSyncApi.Services
{
    public interface IRecipeSyncService
    {
        RecipeSyncResult Save(SaveRecipeRequest request);
        RecipeSyncResult Publish(PublishRecipeRequest request);
        RecipeSyncResult Revoke(RevokeRecipeRequest request);
    }
}
