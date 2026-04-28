using System;
using System.Collections.Generic;

namespace NaturaCo.RecipeEditor.Models
{
    // -----------------------------------------------------------------------
    // Helyi szerkesztoi modell.
    //
    // A klienst sajat editor-allapotaban tartja: a NaturaCo szerver szempontjabol
    // a recept azonositoja a CategoryBvin (lasd CLIENT_APP_CONTEXT.md - "A recept
    // domain modellje ebben a projektben"). RecipeID itt csak helyi sorszam, nem
    // szerver-allapot.
    // -----------------------------------------------------------------------
    public class Recipe
    {
        public int       RecipeID        { get; set; }
        public string    RecipeName      { get; set; }
        public string    ShortDescription{ get; set; }
        public string    Description     { get; set; }
        public string    Category        { get; set; }
        public string    AuthorName      { get; set; }
        public string    PreviewImageUrl { get; set; }
        public string    Tags            { get; set; }
        public int       Servings        { get; set; }
        public int       PrepTimeMinutes { get; set; }
        public int       CookTimeMinutes { get; set; }
        public int?      TotalCalories   { get; set; }
        public string    Steps           { get; set; }
        public string    Status          { get; set; } // Draft | Published | Revoked
        public decimal?  EstimatedCost   { get; set; }

        // A szervertol kapott "horgonyok" - mindket Save utan frissulnek.
        // Publish / Revoke csak ezekkel hivhato.
        public string    CategoryBvin    { get; set; }
        public string    BundleBvin      { get; set; }

        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    }

    // Editor-szintu osszetevo - a GridView ezt jeleniti meg.
    // Sendingkor RecipeIngredientDto-ra kepzodik le.
    public class RecipeIngredient
    {
        public string  IngredientName     { get; set; }
        public decimal Amount             { get; set; }
        public string  Unit               { get; set; }
        public string  ProductBvin        { get; set; } // Hotcakes termek-azonosito (string)
        public int     SortOrder          { get; set; }

        // Csak megjelenitesre / koltsegszamitasra
        public string  LinkedProductName  { get; set; }
        public decimal LinkedProductPrice { get; set; } // Ft / csomag (Hotcakes SitePrice)
        public decimal PricePerGram       { get; set; } // Ft / g - 0 ha nincs gramm-alapu adat
        public decimal CaloriesPer100g    { get; set; } // kcal / 100 g - 0 ha ismeretlen

        // Sor-szintu szamitott kaloria (Amount + Unit + CaloriesPer100g alapjan, vagy szerverrol betoltve)
        public decimal Calories           { get; set; }

        // Csomag metaadatok (API-ba kuldendo)
        public decimal PackageQuantity    { get; set; } // csomag merete (pl. 500)
        public string  PackageUnit        { get; set; } // csomag egysege (pl. "g", "db")
    }

    // -----------------------------------------------------------------------
    // Hotcakes REST DTO-k (csak a katalogus bongeszesehez)
    // -----------------------------------------------------------------------
    public class HccProduct
    {
        public string  Bvin            { get; set; }
        public string  ProductName     { get; set; }
        public string  Sku             { get; set; }
        public decimal SitePrice       { get; set; }
        public string  UrlSlug         { get; set; }
        public string  ImageSmall      { get; set; }
        public decimal CaloriesPer100g { get; set; } // hcc_Product.CustomProperties / Key=CaloriesPer100g
    }

    public class HccCategory
    {
        public string Bvin { get; set; }
        public string Name { get; set; }
    }

    // -----------------------------------------------------------------------
    // NaturaCo RecipeSync API contract (CLIENT_APP_CONTEXT.md alapjan)
    // POST /DesktopModules/NaturaCo/API/RecipeSync/{Save|Publish|Revoke}
    // -----------------------------------------------------------------------

    public class SaveRecipeRequest
    {
        public int?    RecipeId             { get; set; }
        public string  RecipeName           { get; set; }
        public string  MealType             { get; set; } // Reggeli | Ebéd | Vacsora | Snack
        public string  ShortDescription     { get; set; }
        public string  Description          { get; set; }
        public string  Steps                { get; set; }
        public string  Tags                 { get; set; }
        public int     Servings             { get; set; }
        public int     PrepTimeMinutes      { get; set; }
        public int     CookTimeMinutes      { get; set; }
        public int?    TotalCalories        { get; set; }
        public decimal EstimatedCost        { get; set; }
        public string  AuthorName           { get; set; }
        public string  PreviewImageUrl      { get; set; }
        public string  Status               { get; set; }
        public string  CategoryBvin         { get; set; }
        public string  BundleBvin           { get; set; }
        public bool    CreateOrUpdateBundle { get; set; }
        public bool    PublishAfterSave     { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
    }

    public class RecipeIngredientDto
    {
        public string  ProductBvin     { get; set; }
        public string  ProductName     { get; set; }
        public decimal Quantity        { get; set; }
        public string  Unit            { get; set; }
        public int     SortOrder       { get; set; }
        public decimal Calories        { get; set; } // kcal / recept-adag (opcionalis)
        public decimal Price           { get; set; } // Ft / recept-adag (opcionalis)
        public decimal PackageQuantity { get; set; } // csomag mérete (opcionalis)
        public string  PackageUnit     { get; set; } // csomag egysége, pl. "g", "db" (opcionalis)
    }

    public class PublishRecipeRequest
    {
        public int?   RecipeId     { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin   { get; set; }
    }

    public class RevokeRecipeRequest
    {
        public int?   RecipeId     { get; set; }
        public string CategoryBvin { get; set; }
        public string BundleBvin   { get; set; }
    }

    public class RecipeSyncResult
    {
        public bool         Success      { get; set; }
        public int?         RecipeId     { get; set; }
        public string       CategoryBvin { get; set; }
        public string       BundleBvin   { get; set; }
        public string       Status       { get; set; }
        public string       Message      { get; set; }
        public List<string> Errors       { get; set; } = new List<string>();
    }

    // GET /RecipeSync/List vegpont valasza - egy-egy sor a receptlistaban
    public class RecipeListItem
    {
        public int    RecipeId         { get; set; }
        public string RecipeName       { get; set; }
        public string Status           { get; set; }
        public string CategoryBvin     { get; set; }
        public string BundleBvin       { get; set; }
        public string MealType         { get; set; }
        public string ShortDescription { get; set; }
        public int    Servings         { get; set; }
        public int    PrepTimeMinutes  { get; set; }
        public int    CookTimeMinutes  { get; set; }
        public int?   TotalCalories    { get; set; }
    }

    // A szerver {"Success":true,"Recipes":[...]} formaban adja vissza a listat
    public class RecipeListResponse
    {
        public bool             Success { get; set; }
        public List<RecipeListItem> Recipes { get; set; } = new List<RecipeListItem>();
    }

    // GET /RecipeSync/Load?id={id} vegpont valasza - {"Success":true,"Recipe":{...}}
    public class RecipeLoadResponse
    {
        public bool             Success { get; set; }
        public string           Message { get; set; }
        public RecipeLoadResult Recipe  { get; set; }
    }

    public class RecipeLoadResult
    {
        public int                       RecipeId         { get; set; }
        public string                    RecipeName       { get; set; }
        public string                    ShortDescription { get; set; }
        public string                    Description      { get; set; }
        public string                    MealType         { get; set; } // Reggeli / Ebéd / Vacsora / Snack
        public string                    Steps            { get; set; }
        public string                    Tags             { get; set; }
        public int                       Servings         { get; set; }
        public int                       PrepTimeMinutes  { get; set; }
        public int                       CookTimeMinutes  { get; set; }
        public int?                      TotalCalories    { get; set; }
        public decimal?                  EstimatedCost    { get; set; }
        public string                    AuthorName       { get; set; }
        public string                    PreviewImageUrl  { get; set; }
        public string                    Status           { get; set; }
        public string                    CategoryBvin     { get; set; }
        public string                    BundleBvin       { get; set; }
        public List<RecipeIngredientDto> Ingredients      { get; set; } = new List<RecipeIngredientDto>();
    }
}
