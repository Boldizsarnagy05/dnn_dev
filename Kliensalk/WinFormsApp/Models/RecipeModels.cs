using System;
using System.Collections.Generic;

namespace NaturaCo.RecipeEditor.Models
{
    // Recept fő adatai – megfelel a RecipeRecipes táblának
    public class Recipe
    {
        public int       RecipeID        { get; set; }
        public int       PortalID        { get; set; }
        public string    RecipeName      { get; set; }
        public string    Description     { get; set; }
        public string    Category        { get; set; }
        public string    AuthorName      { get; set; }
        public string    PreviewImageURL { get; set; }
        public string    Tags            { get; set; }
        public int       Servings        { get; set; }
        public int       PrepTimeMinutes { get; set; }
        public int       CookTimeMinutes { get; set; }
        public int?      TotalCalories   { get; set; }
        public string    Steps           { get; set; }
        public string    Status          { get; set; } // Draft | Published | Revoked
        public int       CreatedByUserID { get; set; }
        public DateTime  CreatedOnDate   { get; set; }
        public decimal?  EstimatedCost   { get; set; }

        // Bundle hivatkozás – akkor kap értéket, ha a recept publikálva lett
        // és a bundle létrehozása megvalósult
        public Guid?     BundleBvin      { get; set; }

        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }

    // Összetevő – megfelel a RecipeIngredients táblának
    public class RecipeIngredient
    {
        public int     IngredientID   { get; set; }
        public int     RecipeID       { get; set; }
        public string  IngredientName { get; set; }
        public decimal Amount         { get; set; }
        public string  Unit           { get; set; }
        public Guid?   ProductID      { get; set; } // hcc_Product.bvin – opcionális
        public int     SortOrder      { get; set; }

        // Csak megjelenítésre, nem kerül adatbázisba
        public string  LinkedProductName  { get; set; }
        public decimal LinkedProductPrice { get; set; }
    }

    // HotCakes termék – a REST API válaszából töltődik
    public class HccProduct
    {
        public string  Bvin        { get; set; }
        public string  ProductName { get; set; }
        public string  Sku         { get; set; }
        public decimal SitePrice   { get; set; }
        public string  UrlSlug     { get; set; }
        public string  ImageSmall  { get; set; }
    }

    // HotCakes kategória – a REST API válaszából töltődik
    public class HccCategory
    {
        public string Bvin { get; set; }
        public string Name { get; set; }
    }
}
