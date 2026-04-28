namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class RecipeIngredientDto
    {
        public string ProductBvin { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal? Calories { get; set; }
        public decimal? Price { get; set; }
        public decimal? PackageQuantity { get; set; }
        public string PackageUnit { get; set; }
        public int SortOrder { get; set; }
    }
}
