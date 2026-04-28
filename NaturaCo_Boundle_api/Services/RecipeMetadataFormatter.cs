using System;
using System.Linq;
using NaturaCo.RecipeSyncApi.Models;
using Newtonsoft.Json;

namespace NaturaCo.RecipeSyncApi.Services
{
    internal static class RecipeMetadataFormatter
    {
        private const string StartMarker = "<!-- NATURACO_RECIPE_METADATA:";
        private const string EndMarker = ":NATURACO_RECIPE_METADATA -->";

        public static string Apply(string visibleDescription, SaveRecipeRequest request)
        {
            var cleanDescription = Strip(visibleDescription);
            var metadata = new RecipeMetadata
            {
                RecipeId = request.RecipeId,
                MealType = request.MealType,
                ShortDescription = request.ShortDescription,
                Description = request.Description,
                Steps = request.Steps,
                Tags = request.Tags,
                Servings = request.Servings,
                PrepTimeMinutes = request.PrepTimeMinutes,
                CookTimeMinutes = request.CookTimeMinutes,
                TotalCalories = request.TotalCalories ?? 0,
                EstimatedCost = request.EstimatedCost ?? 0m,
                PreviewImageUrl = request.PreviewImageUrl,
                Status = request.PublishAfterSave ? "Published" : request.Status ?? "Draft",
                BundleBvin = request.BundleBvin,
                Ingredients = (request.Ingredients ?? Enumerable.Empty<RecipeIngredientDto>())
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new RecipeIngredientMetadata
                    {
                        ProductBvin = i.ProductBvin,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Calories = i.Calories ?? 0,
                        Price = i.Price ?? 0m,
                        PackageQuantity = i.PackageQuantity ?? 0m,
                        PackageUnit = i.PackageUnit,
                        SortOrder = i.SortOrder
                    })
                    .ToList()
            };

            var json = JsonConvert.SerializeObject(metadata);
            return cleanDescription + Environment.NewLine + StartMarker + json + EndMarker;
        }

        public static string ApplyStatus(string description, string status)
        {
            var cleanDescription = Strip(description);
            var metadata = Extract(description);
            metadata.Status = string.IsNullOrWhiteSpace(status) ? "Draft" : status;

            if (string.IsNullOrWhiteSpace(metadata.Description))
            {
                metadata.Description = cleanDescription;
            }

            var json = JsonConvert.SerializeObject(metadata);
            return cleanDescription + Environment.NewLine + StartMarker + json + EndMarker;
        }

        public static bool ContainsMetadata(string description)
        {
            return !string.IsNullOrWhiteSpace(description)
                && description.IndexOf(StartMarker, StringComparison.Ordinal) >= 0
                && description.IndexOf(EndMarker, StringComparison.Ordinal) >= 0;
        }

        public static RecipeMetadata Extract(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return new RecipeMetadata();
            }

            var start = description.IndexOf(StartMarker, StringComparison.Ordinal);
            if (start < 0)
            {
                return new RecipeMetadata { Description = description };
            }

            start += StartMarker.Length;
            var end = description.IndexOf(EndMarker, start, StringComparison.Ordinal);
            if (end < 0)
            {
                return new RecipeMetadata { Description = Strip(description) };
            }

            var json = description.Substring(start, end - start);
            try
            {
                return JsonConvert.DeserializeObject<RecipeMetadata>(json) ?? new RecipeMetadata { Description = Strip(description) };
            }
            catch
            {
                return new RecipeMetadata { Description = Strip(description) };
            }
        }

        public static string Strip(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            var start = description.IndexOf(StartMarker, StringComparison.Ordinal);
            if (start < 0)
            {
                return description;
            }

            var end = description.IndexOf(EndMarker, start, StringComparison.Ordinal);
            if (end < 0)
            {
                return description.Substring(0, start).Trim();
            }

            return (description.Substring(0, start) + description.Substring(end + EndMarker.Length)).Trim();
        }
    }
}
