using System;
using System.Collections.Generic;
using System.Linq;
using Hotcakes.CommerceDTO.v1.Client;
using NaturaCo.RecipeEditor.Models;

namespace NaturaCo.RecipeEditor.Services
{
    // Wrapper a HotCakes beépített REST API kliens köré.
    // Referencia: Hotcakes.CommerceDTO.dll (a HotCakes telepítőből másolható ki)
    // Útvonal: \DesktopModules\Hotcakes\Core\bin\Hotcakes.CommerceDTO.dll
    public class HotCakesService
    {
        private readonly Api _api;

        // storeUrl  : pl. "https://naturaco.hu"
        // apiKey    : HotCakes Admin → Configuration → API → API Key
        public HotCakesService(string storeUrl, string apiKey)
        {
            _api = new Api(storeUrl, apiKey);
        }

        // ------------------------------------------------------------------
        // Kategóriák
        // ------------------------------------------------------------------

        public List<HccCategory> GetCategories()
        {
            var response = _api.CategoriesFindAll();

            if (response.Errors.Count > 0)
                throw new Exception(string.Join(", ", response.Errors.Select(e => e.Description)));

            return response.Content
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

            foreach (var ing in ingredients.Where(i => i.ProductID.HasValue))
            {
                var product = FindProduct(ing.ProductID.Value.ToString());
                if (product != null)
                    total += product.SitePrice * (decimal)ing.Amount;
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
        // Segéd metódusok
        // ------------------------------------------------------------------

        private static HccProduct MapProduct(Hotcakes.CommerceDTO.v1.Catalog.ProductDTO dto)
        {
            if (dto == null) return null;

            return new HccProduct
            {
                Bvin        = dto.Bvin,
                ProductName = dto.ProductName,
                Sku         = dto.Sku,
                SitePrice   = (decimal)dto.SitePrice,
                UrlSlug     = dto.UrlSlug,
                ImageSmall  = dto.ImageFileSmall
            };
        }
    }
}
