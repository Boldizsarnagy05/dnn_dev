using System.Collections.Generic;

namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class RecipeSyncResult
    {
        public bool Success { get; set; }
        public int? RecipeId { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static RecipeSyncResult Failed(string message, IEnumerable<string> errors = null)
        {
            var result = new RecipeSyncResult
            {
                Success = false,
                Message = message
            };

            if (errors != null)
            {
                result.Errors.AddRange(errors);
            }

            return result;
        }
    }
}
