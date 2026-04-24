using System;
using System.Collections.Generic;
using System.Linq;
using NaturaCo.RecipeSyncApi.Models;

namespace NaturaCo.RecipeSyncApi.Services
{
    public sealed class RecipeSyncService : IRecipeSyncService
    {
        private readonly IHotcakesRecipeGateway _gateway;

        public RecipeSyncService()
            : this(new HotcakesRecipeGateway())
        {
        }

        public RecipeSyncService(IHotcakesRecipeGateway gateway)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public RecipeSyncResult Save(SaveRecipeRequest request)
        {
            var errors = ValidateSave(request);
            if (errors.Count > 0)
            {
                return RecipeSyncResult.Failed("A recept mentese nem sikerult.", errors);
            }

            var category = _gateway.UpsertRecipeCategory(request);
            _gateway.ReplaceCategoryProducts(category.CategoryBvin, request.Ingredients);

            string bundleBvin = request.BundleBvin;
            if (request.CreateOrUpdateBundle)
            {
                bundleBvin = _gateway.UpsertBundle(request, category.CategoryBvin)?.BundleBvin;
            }

            if (request.PublishAfterSave)
            {
                _gateway.Publish(category.CategoryBvin, bundleBvin);
            }

            return new RecipeSyncResult
            {
                Success = true,
                RecipeId = request.RecipeId,
                CategoryBvin = category.CategoryBvin,
                BundleBvin = bundleBvin,
                Status = request.PublishAfterSave ? "Published" : request.Status ?? "Draft",
                Message = "A recept mentese sikeres volt."
            };
        }

        public RecipeSyncResult Publish(PublishRecipeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CategoryBvin))
            {
                return RecipeSyncResult.Failed(
                    "A recept publikalasa nem sikerult.",
                    new[] { "A CategoryBvin kotelezo." });
            }

            _gateway.Publish(request.CategoryBvin, request.BundleBvin);

            return new RecipeSyncResult
            {
                Success = true,
                RecipeId = request.RecipeId,
                CategoryBvin = request.CategoryBvin,
                BundleBvin = request.BundleBvin,
                Status = "Published",
                Message = "A recept publikalasa sikeres volt."
            };
        }

        public RecipeSyncResult Revoke(RevokeRecipeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CategoryBvin))
            {
                return RecipeSyncResult.Failed(
                    "A recept visszavonasa nem sikerult.",
                    new[] { "A CategoryBvin kotelezo." });
            }

            _gateway.Revoke(request.CategoryBvin, request.BundleBvin);

            return new RecipeSyncResult
            {
                Success = true,
                RecipeId = request.RecipeId,
                CategoryBvin = request.CategoryBvin,
                BundleBvin = request.BundleBvin,
                Status = "Revoked",
                Message = "A recept visszavonasa sikeres volt."
            };
        }

        private static List<string> ValidateSave(SaveRecipeRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("A keres torzse hianyzik.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.RecipeName))
            {
                errors.Add("A recept neve kotelezo.");
            }

            if (request.Servings <= 0)
            {
                errors.Add("Az adagok szama legyen nagyobb nullanal.");
            }

            if (request.Ingredients == null || request.Ingredients.Count == 0)
            {
                errors.Add("Legalabb egy hozzavalo szukseges.");
                return errors;
            }

            foreach (var ingredient in request.Ingredients.OrderBy(i => i.SortOrder))
            {
                if (string.IsNullOrWhiteSpace(ingredient.ProductBvin))
                {
                    errors.Add("Minden hozzavalhoz tartoznia kell ProductBvin erteknek.");
                }

                if (ingredient.Quantity <= 0)
                {
                    errors.Add($"A(z) {ingredient.ProductName ?? "ismeretlen"} mennyisege legyen nagyobb nullanal.");
                }
            }

            return errors;
        }
    }
}
