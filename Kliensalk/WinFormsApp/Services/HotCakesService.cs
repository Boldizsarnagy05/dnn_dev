using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Hotcakes.CommerceDTO.v1.Client;
using Newtonsoft.Json;
using NaturaCo.RecipeEditor.Models;

namespace NaturaCo.RecipeEditor.Services
{
    // Wrapper a HotCakes beépített REST API kliens köré.
    // Referencia: Hotcakes.CommerceDTO.dll (a HotCakes telepítőből másolható ki)
    // Útvonal: \DesktopModules\Hotcakes\Core\bin\Hotcakes.CommerceDTO.dll
    public class HotCakesService
    {
        private readonly Api        _api;
        private readonly HttpClient _http;
        private readonly string     _storeUrl;
        private readonly string     _apiKey;

        // storeUrl  : pl. "https://naturaco.hu"
        // apiKey    : HotCakes Admin → Configuration → API → API Key
        public HotCakesService(string storeUrl, string apiKey)
        {
            _storeUrl = (storeUrl ?? string.Empty).TrimEnd('/');
            _apiKey   = apiKey;
            _api      = new Api(storeUrl, apiKey);
            _http     = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        // ------------------------------------------------------------------
        // Kategóriák
        // ------------------------------------------------------------------

        public List<HccCategory> GetCategories(HashSet<string> excludeBvins = null)
        {
            var response = _api.CategoriesFindAll();

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            // Csak latható (Hidden=false) kategoriak, a recept-kategoriak kizarasaval.
            return response.Content
                .Where(c => !c.Hidden)
                .Where(c => excludeBvins == null || !excludeBvins.Contains(c.Bvin))
                .Select(c => new HccCategory
                {
                    Bvin = c.Bvin,
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .ToList();
        }

        // ------------------------------------------------------------------
        // Termékek
        // ------------------------------------------------------------------

        // A NaturaCo modul minden recepthez Hidden=true Hotcakes kategoriat hoz letre.
        // Ezzel az applikacio-inditaskor betolthetjuk az osszes korabban mentett receptet
        // akkor is, ha a szerver /List vegpont meg nincs implementalva.
        public List<RecipeListItem> GetAllRecipesFromHcc()
        {
            var response = _api.CategoriesFindAll();

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            return response.Content
                .Where(c => c.Hidden)
                .Select(c => new RecipeListItem
                {
                    RecipeId     = 0,
                    RecipeName   = c.Name,
                    Status       = "?",
                    CategoryBvin = c.Bvin,
                    BundleBvin   = string.Empty
                })
                .OrderBy(c => c.RecipeName)
                .ToList();
        }

        public List<HccProduct> GetProductsByCategory(string categoryBvin, int page = 1, int pageSize = 50)
        {
            var response = _api.ProductsFindForCategory(categoryBvin, page, pageSize);

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            return response.Content.Products
                .Select(MapProduct)
                .ToList();
        }

        public List<HccProduct> GetAllProducts(int page = 1, int pageSize = 100)
        {
            var response = _api.ProductsFindPage(page, pageSize);

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            return response.Content.Products
                .Select(MapProduct)
                .ToList();
        }

        public HccProduct FindProduct(string bvin)
        {
            var response = _api.ProductsFind(bvin);

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            return MapProduct(response.Content);
        }

        public HccProduct FindProductBySlug(string slug)
        {
            var response = _api.ProductsBySlug(slug);

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            return MapProduct(response.Content);
        }

        // ------------------------------------------------------------------
        // Tápérték és ár számítás összetevőkből
        // ------------------------------------------------------------------

        public decimal CalculateEstimatedCost(List<RecipeIngredient> ingredients)
        {
            decimal total = 0;

            foreach (var ing in ingredients.Where(i => !string.IsNullOrEmpty(i.ProductBvin)))
            {
                var product = FindProduct(ing.ProductBvin);
                if (product != null)
                    total += product.SitePrice * ing.Amount;
            }

            return total;
        }

        // ------------------------------------------------------------------
        // Bundle létrehozás – EGYELŐRE KOMMENTÁLVA
        // A HotCakes REST API nem tartalmaz bundle végpontot:
        // - A ProductDTO-ban nincs IsBundle mező
        // - Nincs BundledProductsCreate() metódus az Api osztályban
        // - A hcc_BundledProducts tábla csak belső SDK-n vagy direkt SQL-en keresztül írható
        // Megvalósítás: saját DNN Web API végpont szükséges
        // ------------------------------------------------------------------

        /*
        public Guid CreateBundleFromRecipe(Recipe recipe)
        {
            // 1. Bundle termék létrehozása a REST API-val
            var bundleDto = new ProductDTO
            {
                ProductName        = recipe.RecipeName,
                LongDescription    = recipe.Description,
                SitePrice          = (double)(recipe.EstimatedCost ?? 0),
                UrlSlug            = Slugify(recipe.RecipeName) + "-csomag",
                Status             = ProductStatusDTO.Active,
                IsAvailableForSale = true,
                IsSearchable       = true
                // IsBundle = true  ← NEM ELÉRHETŐ a ProductDTO-ban
            };

            var result = _api.ProductsCreate(bundleDto, null);
            var bundleBvin = Guid.Parse(result.Content.Bvin);

            // 2. Bundle flag + összetevők beállítása
            // ← NINCS REST végpont erre, saját DNN Web API kell:
            // POST /api/RecipeModule/Recipe/{id}/bundle-finalize
            // Body: { bundleBvin, ingredients }

            return bundleBvin;
        }
        */

        // ------------------------------------------------------------------
        // Csomag-opciok -> Ft/g
        //
        // A Hotcakes /productoptions?byproduct={bvin} vegpont strukturaltan
        // visszaadja a "Csomag:" opciot az Items[]-vel ("500 g", "150 g", ...).
        // A default item (IsDefault=true) nevebol kiparsoljuk a grammot,
        // es az adott termek SitePrice-abol Ft/g-ot szamolunk.
        //
        // Ha barmi nem stimmel (nincs option, nem g/kg egyseg, stb.) -> 0,
        // ekkor a UI darab-logikara esik vissza.
        // ------------------------------------------------------------------

        public decimal GetPricePerGramOrZero(HccProduct product)
        {
            if (product == null || string.IsNullOrEmpty(product.Bvin))
                return 0m;

            try
            {
                var url = $"{_storeUrl}/DesktopModules/Hotcakes/API/rest/v1/productoptions" +
                          $"?key={Uri.EscapeDataString(_apiKey)}" +
                          $"&byproduct={Uri.EscapeDataString(product.Bvin)}";

                var json = _http.GetStringAsync(url).GetAwaiter().GetResult();
                var resp = JsonConvert.DeserializeObject<HccOptionsResponse>(json);
                if (resp?.Content == null) return 0m;

                // A "Csomag:" nev a NaturaCo storeban hasznalt - de altalaban az elso
                // option az, aminek vannak ar/suly modositasos itemei.
                var packageOption = resp.Content
                    .FirstOrDefault(o => o.Items != null && o.Items.Any(i => i.IsDefault))
                    ?? resp.Content.FirstOrDefault();

                var defaultItem = packageOption?.Items?.FirstOrDefault(i => i.IsDefault)
                                  ?? packageOption?.Items?.FirstOrDefault();

                if (defaultItem == null) return 0m;

                var grams = ParseGrams(defaultItem.Name);
                if (grams <= 0) return 0m;

                var defaultPrice = product.SitePrice + (decimal)defaultItem.PriceAdjustment;
                if (defaultPrice <= 0) return 0m;

                return defaultPrice / grams;
            }
            catch
            {
                return 0m;
            }
        }

        // "500 g" -> 500, "1 kg" -> 1000, "250 ml" -> 0 (nem gramm), stb.
        private static decimal ParseGrams(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName)) return 0m;

            var m = Regex.Match(
                itemName,
                @"(?<num>\d+(?:[.,]\d+)?)\s*(?<unit>kg|g)\b",
                RegexOptions.IgnoreCase);

            if (!m.Success) return 0m;

            var numText = m.Groups["num"].Value.Replace(',', '.');
            if (!decimal.TryParse(numText, NumberStyles.Number, CultureInfo.InvariantCulture, out var num))
                return 0m;

            var unit = m.Groups["unit"].Value.ToLowerInvariant();
            return unit == "kg" ? num * 1000m : num;
        }

        // Minimal DTO-k a productoptions valaszhoz
        private class HccOptionsResponse
        {
            public List<HccOption> Content { get; set; }
        }

        private class HccOption
        {
            public string             Name  { get; set; }
            public List<HccOptionItem> Items { get; set; }
        }

        private class HccOptionItem
        {
            public string  Name            { get; set; }
            public double  PriceAdjustment { get; set; }
            public bool    IsDefault       { get; set; }
        }

        // ------------------------------------------------------------------
        // Segéd metódusok
        // ------------------------------------------------------------------

        private static HccProduct MapProduct(Hotcakes.CommerceDTO.v1.Catalog.ProductDTO dto)
        {
            if (dto == null) return null;

            return new HccProduct
            {
                Bvin            = dto.Bvin,
                ProductName     = dto.ProductName,
                Sku             = dto.Sku,
                SitePrice       = (decimal)dto.SitePrice,
                UrlSlug         = dto.UrlSlug,
                ImageSmall      = dto.ImageFileSmall,
                CaloriesPer100g = ExtractCalories(dto.CustomProperties)
            };
        }

        // hcc_Product.CustomProperties XML-bol kerul ide a Key="CaloriesPer100g".
        // Az ertek string, magyaroszagi vesszovel vagy ponttal jonhet (pl. "362").
        private static decimal ExtractCalories(IEnumerable<Hotcakes.CommerceDTO.v1.CustomPropertyDTO> props)
        {
            if (props == null) return 0m;

            foreach (var p in props)
            {
                if (p == null) continue;
                if (!string.Equals(p.Key, "CaloriesPer100g", StringComparison.OrdinalIgnoreCase))
                    continue;

                var raw = (p.Value ?? string.Empty).Trim().Replace(',', '.');
                if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var kcal))
                    return kcal;
            }
            return 0m;
        }
    }
}
