using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Teszko.ReceptModulRecept_modul.Models;

namespace Teszko.ReceptModulRecept_modul.Components
{
    internal interface IRecipeStorefrontService
    {
        RecipeListViewModel GetRecipes(string mealType);
        RecipeDetailViewModel GetRecipe(string categoryBvin);
        CartActionResult AddToCart(IEnumerable<CartLineRequest> items);
    }

    internal sealed class RecipeStorefrontService : IRecipeStorefrontService
    {
        public RecipeListViewModel GetRecipes(string mealType)
        {
            var recipes = TryLoadHotcakesRecipes();
            if (recipes == null)
            {
                recipes = DemoRecipes();
            }

            if (!string.IsNullOrWhiteSpace(mealType) && !mealType.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                recipes = recipes
                    .Where(r => string.Equals(r.MealType, mealType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return new RecipeListViewModel
            {
                ActiveMealType = string.IsNullOrWhiteSpace(mealType) ? "all" : mealType,
                Recipes = recipes
            };
        }

        public RecipeDetailViewModel GetRecipe(string categoryBvin)
        {
            if (string.IsNullOrWhiteSpace(categoryBvin))
            {
                return null;
            }

            var recipe = TryLoadHotcakesRecipe(categoryBvin);
            if (recipe != null)
            {
                return recipe;
            }

            return DemoRecipeDetails().FirstOrDefault(r => string.Equals(r.CategoryBvin, categoryBvin, StringComparison.OrdinalIgnoreCase));
        }

        public CartActionResult AddToCart(IEnumerable<CartLineRequest> items)
        {
            var lines = (items ?? Enumerable.Empty<CartLineRequest>())
                .Where(i => !string.IsNullOrWhiteSpace(i.ProductBvin) && i.Quantity > 0)
                .ToList();

            if (lines.Count == 0)
            {
                return new CartActionResult
                {
                    Success = false,
                    Message = "Nincs kosárba tehető termék.",
                    Errors = { "Válassz legalább egy hozzávalót." }
                };
            }

            if (!TryGetHotcakesApp(out var hccApp))
            {
                return new CartActionResult
                {
                    Success = true,
                    Message = "A termékek bekerültek a kosár előnézetbe. Hotcakes context nélkül demo módban futott a művelet."
                };
            }

            var errors = new List<string>();
            foreach (var line in lines)
            {
                try
                {
                    if (!TryAddLineToCart(hccApp, line))
                    {
                        errors.Add("Nem sikerült kosárba tenni: " + line.ProductBvin);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(line.ProductBvin + ": " + ex.Message);
                }
            }

            return new CartActionResult
            {
                Success = errors.Count == 0,
                Message = errors.Count == 0 ? "A kiválasztott termékek bekerültek a kosárba." : "Nem minden terméket sikerült kosárba tenni.",
                Errors = errors
            };
        }

        private static List<RecipeCardViewModel> TryLoadHotcakesRecipes()
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                return null;
            }

            var catalog = GetPropertyIfPresent(hccApp, "CatalogServices");
            var categories = GetPropertyIfPresent(catalog, "Categories");
            var list = TryInvokeAny(categories, new[] { "FindAll" }) as IEnumerable;
            if (list == null)
            {
                return null;
            }

            var recipes = new List<RecipeCardViewModel>();
            foreach (var category in list)
            {
                var description = Convert.ToString(GetPropertyIfPresent(category, "Description"));
                if (!IsPublishedRecipe(description))
                {
                    continue;
                }

                var card = MapCard(category);
                if (!string.IsNullOrWhiteSpace(card.CategoryBvin))
                {
                    recipes.Add(card);
                }
            }

            return recipes.OrderBy(r => r.Name).ToList();
        }

        private static RecipeDetailViewModel TryLoadHotcakesRecipe(string categoryBvin)
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                return null;
            }

            var catalog = GetPropertyIfPresent(hccApp, "CatalogServices");
            var categories = GetPropertyIfPresent(catalog, "Categories");
            var category = TryInvokeAny(categories, new[] { "Find", "FindWithCache" }, categoryBvin);
            if (category == null)
            {
                return null;
            }

            var description = Convert.ToString(GetPropertyIfPresent(category, "Description"));
            if (!IsPublishedRecipe(description))
            {
                return null;
            }

            var detail = MapDetail(category);
            FillHotcakesProducts(catalog, detail);
            return detail;
        }

        private static bool IsPublishedRecipe(string description)
        {
            if (!RecipeMetadataFormatter.ContainsMetadata(description))
            {
                return false;
            }

            var meta = RecipeMetadataFormatter.Extract(description);
            return string.Equals(meta.Status, "Published", StringComparison.OrdinalIgnoreCase);
        }

        private static RecipeCardViewModel MapCard(object category)
        {
            var description = Convert.ToString(GetPropertyIfPresent(category, "Description"));
            var meta = RecipeMetadataFormatter.Extract(description);
            var fallbackDescription = RecipeMetadataFormatter.Strip(description);
            var servings = meta.Servings <= 0 ? 1 : meta.Servings;

            return new RecipeCardViewModel
            {
                CategoryBvin = Convert.ToString(GetPropertyIfPresent(category, "Bvin")),
                Name = Convert.ToString(GetPropertyIfPresent(category, "Name")),
                ShortDescription = meta.ShortDescription ?? Convert.ToString(GetPropertyIfPresent(category, "MetaDescription")) ?? fallbackDescription,
                MealType = NormalizeMealType(meta.MealType),
                Servings = servings,
                PrepTimeMinutes = meta.PrepTimeMinutes,
                CookTimeMinutes = meta.CookTimeMinutes,
                TotalCalories = meta.TotalCalories
            };
        }

        private static RecipeDetailViewModel MapDetail(object category)
        {
            var card = MapCard(category);
            var description = Convert.ToString(GetPropertyIfPresent(category, "Description"));
            var meta = RecipeMetadataFormatter.Extract(description);

            return new RecipeDetailViewModel
            {
                CategoryBvin = card.CategoryBvin,
                Name = card.Name,
                Description = !string.IsNullOrWhiteSpace(meta.Description) ? meta.Description : RecipeMetadataFormatter.Strip(description),
                MealType = card.MealType,
                Servings = card.Servings,
                PrepTimeMinutes = card.PrepTimeMinutes,
                CookTimeMinutes = card.CookTimeMinutes,
                TotalCalories = card.TotalCalories,
                EstimatedCost = meta.EstimatedCost,
                Steps = SplitSteps(meta.Steps),
                Ingredients = OrderIngredients(meta.Ingredients
                    .Select(i => new RecipeIngredientViewModel
                    {
                        ProductBvin = i.ProductBvin,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Calories = i.Calories,
                        Price = i.Price,
                        PackageQuantity = i.PackageQuantity,
                        PackageUnit = i.PackageUnit,
                        SortOrder = i.SortOrder
                    }))
                    .ToList()
            };
        }

        private static void FillHotcakesProducts(object catalog, RecipeDetailViewModel detail)
        {
            var productsService = GetPropertyIfPresent(catalog, "Products");
            var relations = GetPropertyIfPresent(catalog, "CategoriesXProducts");
            var associations = TryInvokeAny(relations, new[] { "FindForCategory" }, detail.CategoryBvin, 1, 500) as IEnumerable;
            if (associations == null)
            {
                return;
            }

            foreach (var relation in associations)
            {
                var productBvin = Convert.ToString(GetPropertyIfPresent(relation, "ProductId"));
                if (string.IsNullOrWhiteSpace(productBvin))
                {
                    continue;
                }

                var existing = detail.Ingredients.FirstOrDefault(i => string.Equals(i.ProductBvin, productBvin, StringComparison.OrdinalIgnoreCase));
                var product = TryInvokeAny(productsService, new[] { "Find", "FindWithCache" }, productBvin);
                if (existing == null)
                {
                    existing = new RecipeIngredientViewModel
                    {
                        ProductBvin = productBvin,
                        Quantity = 1,
                        Unit = "db",
                        SortOrder = Convert.ToInt32(GetPropertyIfPresent(relation, "SortOrder") ?? 0)
                    };
                    detail.Ingredients.Add(existing);
                }

                if (product != null)
                {
                    existing.ProductName = existing.ProductName ?? Convert.ToString(GetPropertyIfPresent(product, "ProductName"));
                    existing.Price = existing.Price > 0 ? existing.Price : Convert.ToDecimal(GetPropertyIfPresent(product, "SitePrice") ?? 0m);
                }
            }

            detail.Ingredients = OrderIngredients(detail.Ingredients).ToList();
            if (detail.EstimatedCost <= 0)
            {
                detail.EstimatedCost = detail.Ingredients.Sum(i => i.Price);
            }
        }

        private static IEnumerable<RecipeIngredientViewModel> OrderIngredients(IEnumerable<RecipeIngredientViewModel> ingredients)
        {
            return (ingredients ?? Enumerable.Empty<RecipeIngredientViewModel>())
                .OrderBy(i => string.IsNullOrWhiteSpace(i.ProductBvin) ? 1 : 0)
                .ThenBy(i => i.SortOrder)
                .ThenBy(i => i.ProductName);
        }

        private static bool TryAddLineToCart(object hccApp, CartLineRequest line)
        {
            var orderServices = GetPropertyIfPresent(hccApp, "OrderServices");
            var catalogServices = GetPropertyIfPresent(hccApp, "CatalogServices");
            var products = GetPropertyIfPresent(catalogServices, "Products");
            var product = TryInvokeAny(products, new[] { "Find", "FindWithCache" }, line.ProductBvin);
            if (orderServices == null || product == null)
            {
                return false;
            }

            var cart = TryInvokeAny(orderServices, new[] { "EnsureShoppingCart", "CurrentShoppingCart" });
            if (cart == null)
            {
                return false;
            }

            var lineItemType = GetRequiredType("Hotcakes.Commerce.Orders.LineItem");
            var lineItem = Activator.CreateInstance(lineItemType);
            SetPropertyIfPresent(lineItem, "ProductId", Convert.ToString(GetPropertyIfPresent(product, "Bvin")));
            SetPropertyIfPresent(lineItem, "ProductName", Convert.ToString(GetPropertyIfPresent(product, "ProductName")));
            SetPropertyIfPresent(lineItem, "ProductSku", Convert.ToString(GetPropertyIfPresent(product, "Sku")));
            SetPropertyIfPresent(lineItem, "Quantity", line.Quantity);
            SetPropertyIfPresent(lineItem, "BasePricePerItem", Convert.ToDecimal(GetPropertyIfPresent(product, "SitePrice") ?? 0m));
            SetPropertyIfPresent(lineItem, "AdjustedPricePerItem", Convert.ToDecimal(GetPropertyIfPresent(product, "SitePrice") ?? 0m));

            if (HasPublicMethod(hccApp, new[] { "AddToOrderWithCalculateAndSave", "AddToOrderAndSave" }, 2))
            {
                var result = TryInvokeAny(hccApp, new[] { "AddToOrderWithCalculateAndSave", "AddToOrderAndSave" }, cart, lineItem);
                return result == null || !(result is bool) || (bool)result;
            }

            var added = TryInvokeAny(orderServices, new[] { "AddItemToOrder" }, cart, lineItem);
            if (added is bool && !(bool)added)
            {
                return false;
            }

            var saved = TryInvokeAny(hccApp, new[] { "CalculateOrderAndSave", "CalculateOrderAndSaveWithoutRepricing" }, cart);
            if (saved != null)
            {
                return !(saved is bool) || (bool)saved;
            }

            var orders = GetPropertyIfPresent(orderServices, "Orders");
            var updated = TryInvokeAny(orders, new[] { "Update" }, cart, true);
            TryInvokeAny(orderServices, new[] { "InvalidateCachedCart" });
            return updated == null || !(updated is bool) || (bool)updated;
        }

        private static List<string> SplitSteps(string steps)
        {
            if (string.IsNullOrWhiteSpace(steps))
            {
                return new List<string>();
            }

            return steps
                .Split(new[] { "\r\n", "\n", "|" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
        }

        private static string NormalizeMealType(string mealType)
        {
            if (string.IsNullOrWhiteSpace(mealType))
            {
                return "snack";
            }

            var value = mealType.Trim().ToLowerInvariant();
            if (value.Contains("regg") || value == "breakfast")
            {
                return "reggeli";
            }

            if (value.Contains("ebed") || value.Contains("ebéd") || value == "lunch")
            {
                return "ebed";
            }

            if (value.Contains("vacs") || value == "dinner")
            {
                return "vacsora";
            }

            return "snack";
        }

        private static List<RecipeCardViewModel> DemoRecipes()
        {
            return DemoRecipeDetails().Select(r => new RecipeCardViewModel
            {
                CategoryBvin = r.CategoryBvin,
                Name = r.Name,
                ShortDescription = r.Description,
                MealType = r.MealType,
                Servings = r.Servings,
                PrepTimeMinutes = r.PrepTimeMinutes,
                CookTimeMinutes = r.CookTimeMinutes,
                TotalCalories = r.TotalCalories
            }).ToList();
        }

        private static List<RecipeDetailViewModel> DemoRecipeDetails()
        {
            return new List<RecipeDetailViewModel>
            {
                new RecipeDetailViewModel
                {
                    CategoryBvin = "demo-avokados-toast",
                    Name = "Avokados power toast",
                    Description = "Ez az avokados power toast egy gyors, taplalo reggeli, amely tele van egeszseges zsirokkal es rostokkal.",
                    MealType = "reggeli",
                    Servings = 3,
                    PrepTimeMinutes = 15,
                    TotalCalories = 320,
                    EstimatedCost = 2510m,
                    Ingredients =
                    {
                        new RecipeIngredientViewModel { ProductBvin = "demo-avokado", ProductName = "Avokado", Quantity = 1, Unit = "db", Calories = 80, Price = 2450m, SortOrder = 1 },
                        new RecipeIngredientViewModel { ProductBvin = "demo-kenyer", ProductName = "Kenyer", Quantity = 2, Unit = "szelet", Calories = 70, Price = 0m, SortOrder = 2 },
                        new RecipeIngredientViewModel { ProductBvin = "demo-so", ProductName = "So", Quantity = 1, Unit = "csipet", Calories = 0, Price = 60m, SortOrder = 3 },
                        new RecipeIngredientViewModel { ProductBvin = "demo-citromle", ProductName = "Citromle", Quantity = 0.5m, Unit = "db", Calories = 15, Price = 0m, SortOrder = 4 },
                        new RecipeIngredientViewModel { ProductBvin = "demo-olivaolaj", ProductName = "Olivaolaj", Quantity = 1, Unit = "ek", Calories = 45, Price = 0m, SortOrder = 5 }
                    }
                },
                new RecipeDetailViewModel { CategoryBvin = "demo-gorog-salata", Name = "Gorog salata fetaval", MealType = "ebed", Servings = 2, PrepTimeMinutes = 10, TotalCalories = 180, Description = "Konnyu, friss salata fetaval es premium alapanyagokkal." },
                new RecipeDetailViewModel { CategoryBvin = "demo-wrap", Name = "Csirkes zoldseg wrap", MealType = "ebed", Servings = 4, PrepTimeMinutes = 25, TotalCalories = 420, Description = "Gyors, laktato wrap sok zoldseggel." },
                new RecipeDetailViewModel { CategoryBvin = "demo-granola", Name = "Granola joghurt bowl", MealType = "reggeli", Servings = 1, PrepTimeMinutes = 5, TotalCalories = 380, Description = "Ropogos granola kremes joghurttal." },
                new RecipeDetailViewModel { CategoryBvin = "demo-lencse", Name = "Voroslencse leves", MealType = "ebed", Servings = 4, PrepTimeMinutes = 40, TotalCalories = 290, Description = "Melengeto, tartalmas leves voroslencsebol." },
                new RecipeDetailViewModel { CategoryBvin = "demo-lazac", Name = "Parolt lazac brokkolival", MealType = "vacsora", Servings = 2, PrepTimeMinutes = 20, TotalCalories = 350, Description = "Konnyed vacsora parolt zoldseggel." },
                new RecipeDetailViewModel { CategoryBvin = "demo-humusz", Name = "Humusz piritossal", MealType = "snack", Servings = 2, PrepTimeMinutes = 5, TotalCalories = 260, Description = "Kremes humusz friss piritossal." },
                new RecipeDetailViewModel { CategoryBvin = "demo-turmix", Name = "Tropusi gyumolcs turmix", MealType = "snack", Servings = 1, PrepTimeMinutes = 5, TotalCalories = 150, Description = "Frissito, gyumolcsos turmix." }
            };
        }

        private static bool TryGetHotcakesApp(out object hccApp)
        {
            hccApp = null;
            TryLoadAssembly("Hotcakes.Commerce");
            TryLoadAssembly("Hotcakes.CommerceDTO");
            TryLoadAssembly("Hotcakes.Commerce.Dnn");

            try
            {
                var portalSettings = PortalSettings.Current ?? Globals.GetPortalSettings();
                if (portalSettings == null)
                {
                    return false;
                }

                var dnnGlobalType = GetRequiredType("Hotcakes.Commerce.Dnn.DnnGlobal");
                var setPortalSettings = dnnGlobalType.GetMethod("SetPortalSettings", BindingFlags.Public | BindingFlags.Static);
                setPortalSettings?.Invoke(null, new object[] { portalSettings });

                var factoryType = GetRequiredType("Hotcakes.Commerce.Factory");
                var dnnFactoryType = GetRequiredType("Hotcakes.Commerce.Dnn.DnnFactory");
                var factoryInstance = Activator.CreateInstance(dnnFactoryType);
                factoryType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, factoryInstance, null);
                if (HttpContext.Current != null)
                {
                    factoryType.GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, HttpContext.Current, null);
                }

                var requestContextType = GetRequiredType("Hotcakes.Commerce.HccRequestContext");
                var requestContext = Activator.CreateInstance(requestContextType, new object[] { portalSettings.CultureCode ?? portalSettings.DefaultLanguage ?? "hu-HU" });
                var resolvedStore = ResolveCurrentStore(requestContext, portalSettings);
                if (resolvedStore != null)
                {
                    SetPropertyIfPresent(requestContext, "CurrentStore", resolvedStore);
                }

                if (HttpContext.Current != null)
                {
                    var routeContext = new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData());
                    SetPropertyIfPresent(requestContext, "RoutingContext", routeContext);
                }

                requestContextType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, requestContext, null);

                var contextUtilsType = GetRequiredType("Hotcakes.Commerce.Utilities.HccRequestContextUtils");
                contextUtilsType.GetMethod("UpdateContextStore", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new[] { requestContext });

                var appType = GetRequiredType("Hotcakes.Commerce.HotcakesApplication");
                hccApp = Activator.CreateInstance(appType, new[] { requestContext });
                if (resolvedStore != null)
                {
                    SetPropertyIfPresent(hccApp, "CurrentStore", resolvedStore);
                }

                appType.GetMethod("UpdateCurrentStore", BindingFlags.Public | BindingFlags.Instance)?.Invoke(hccApp, null);
                return GetPropertyIfPresent(hccApp, "CurrentStore") != null || GetPropertyIfPresent(requestContext, "CurrentStore") != null;
            }
            catch
            {
                hccApp = null;
                return false;
            }
        }

        private static object ResolveCurrentStore(object requestContext, PortalSettings portalSettings)
        {
            try
            {
                var storeRepositoryType = GetRequiredType("Hotcakes.Commerce.Accounts.StoreRepository");
                var storeRepository = Activator.CreateInstance(storeRepositoryType, new[] { requestContext });
                var hostName = HttpContext.Current?.Request?.Url?.Host ?? portalSettings?.PortalAlias?.HTTPAlias;
                if (!string.IsNullOrWhiteSpace(hostName))
                {
                    var storeId = TryInvokeAny(storeRepository, new[] { "FindStoreIdByCustomUrl" }, hostName);
                    if (storeId != null && Convert.ToInt64(storeId) > 0)
                    {
                        return TryInvokeAny(storeRepository, new[] { "FindById", "FindByIdWithCache" }, Convert.ToInt64(storeId));
                    }
                }

                var stores = TryInvokeAny(storeRepository, new[] { "ListAllForSuper" }) as IEnumerable;
                if (stores == null)
                {
                    return null;
                }

                object first = null;
                var count = 0;
                foreach (var store in stores)
                {
                    var id = Convert.ToInt64(GetPropertyIfPresent(store, "Id") ?? 0);
                    if (id <= 0)
                    {
                        continue;
                    }

                    first = first ?? store;
                    count++;
                }

                return count == 1 ? first : null;
            }
            catch
            {
                return null;
            }
        }

        private static void TryLoadAssembly(string assemblyName)
        {
            try
            {
                Assembly.Load(assemblyName);
            }
            catch
            {
            }
        }

        private static Type GetRequiredType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .FirstOrDefault(t => string.Equals(t.FullName, fullName, StringComparison.Ordinal));
            if (type == null)
            {
                throw new InvalidOperationException("Hianyzo Hotcakes tipus: " + fullName);
            }

            return type;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        private static object GetPropertyIfPresent(object target, string propertyName)
        {
            if (target == null)
            {
                return null;
            }

            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(target, null);
        }

        private static void SetPropertyIfPresent(object target, string propertyName, object value)
        {
            if (target == null)
            {
                return;
            }

            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite)
            {
                return;
            }

            property.SetValue(target, ConvertValue(value, property.PropertyType), null);
        }

        private static object TryInvokeAny(object target, IReadOnlyCollection<string> methodNames, params object[] args)
        {
            if (target == null)
            {
                return null;
            }

            var methods = target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => methodNames.Contains(m.Name, StringComparer.OrdinalIgnoreCase) && m.GetParameters().Length == args.Length);

            foreach (var method in methods)
            {
                try
                {
                    var parameters = method.GetParameters();
                    var converted = new object[args.Length];
                    for (var i = 0; i < args.Length; i++)
                    {
                        converted[i] = ConvertValue(args[i], parameters[i].ParameterType);
                    }

                    return method.Invoke(target, converted);
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool HasPublicMethod(object target, IReadOnlyCollection<string> methodNames, int argumentCount)
        {
            if (target == null)
            {
                return false;
            }

            return target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Any(m => methodNames.Contains(m.Name, StringComparer.OrdinalIgnoreCase) && m.GetParameters().Length == argumentCount);
        }

        private static object ConvertValue(object value, Type destinationType)
        {
            if (value == null)
            {
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            var targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, Convert.ToString(value), true);
            }

            return Convert.ChangeType(value, targetType);
        }
    }
}
