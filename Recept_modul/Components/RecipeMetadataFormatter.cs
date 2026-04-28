using System;
using Newtonsoft.Json;
using Teszko.ReceptModulRecept_modul.Models;

namespace Teszko.ReceptModulRecept_modul.Components
{
    internal static class RecipeMetadataFormatter
    {
        public const string StartMarker = "<!-- NATURACO_RECIPE_METADATA:";
        public const string EndMarker = ":NATURACO_RECIPE_METADATA -->";

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
