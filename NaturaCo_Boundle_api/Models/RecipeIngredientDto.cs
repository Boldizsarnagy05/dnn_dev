namespace NaturaCo.RecipeSyncApi.Models
{
    public sealed class RecipeIngredientDto
    {
        public string ProductBvin { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public int SortOrder { get; set; }
    }
}
