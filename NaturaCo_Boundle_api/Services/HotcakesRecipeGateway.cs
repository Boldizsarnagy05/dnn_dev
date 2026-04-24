using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NaturaCo.RecipeSyncApi.Models;

namespace NaturaCo.RecipeSyncApi.Services
{
    public sealed class HotcakesRecipeGateway : IHotcakesRecipeGateway
    {
        private readonly FallbackStore _fallbackStore = new FallbackStore();

        public CategorySyncReference UpsertRecipeCategory(SaveRecipeRequest request)
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                return _fallbackStore.UpsertRecipeCategory(request);
            }

            var catalogServices = GetRequiredProperty(hccApp, "CatalogServices");
            var categories = GetRequiredProperty(catalogServices, "Categories");
            var category = FindCategory(categories, request);

            if (category == null)
            {
                category = Activator.CreateInstance(GetRequiredType("Hotcakes.Commerce.Catalog.Category"));
                ApplyCategoryValues(category, request);
                var created = InvokeAny(categories, new[] { "Create" }, category);
                EnsureOperationSucceeded(created, "A recept kategoria letrehozasa nem sikerult.");
            }
            else
            {
                ApplyCategoryValues(category, request);
                var updated = TryInvokeAny(catalogServices, new[] { "CategoryUpdate", "UpdateCategory" }, category)
                    ?? TryInvokeAny(categories, new[] { "Update" }, category);
                EnsureOperationSucceeded(updated, "A recept kategoria frissitese nem sikerult.");
            }

            return new CategorySyncReference
            {
                CategoryBvin = Convert.ToString(GetRequiredProperty(category, "Bvin"))
            };
        }

        public void ReplaceCategoryProducts(string categoryBvin, IReadOnlyCollection<RecipeIngredientDto> ingredients)
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                _fallbackStore.ReplaceCategoryProducts(categoryBvin, ingredients);
                return;
            }

            var catalogServices = GetRequiredProperty(hccApp, "CatalogServices");
            var relations = GetRequiredProperty(catalogServices, "CategoriesXProducts");

            if (TryInvokeAny(relations, new[] { "DeleteAllForCategory" }, categoryBvin) == null)
            {
                DeleteCategoryAssociations(relations, categoryBvin);
            }

            var associationType = GetRequiredType("Hotcakes.Commerce.Catalog.CategoryProductAssociation");
            foreach (var ingredient in ingredients.OrderBy(i => i.SortOrder))
            {
                var association = Activator.CreateInstance(associationType);
                SetPropertyIfPresent(association, "CategoryId", categoryBvin);
                SetPropertyIfPresent(association, "ProductId", ingredient.ProductBvin);
                SetPropertyIfPresent(association, "SortOrder", ingredient.SortOrder);

                var created = TryInvokeAny(relations, new[] { "Create", "Add" }, association);
                EnsureOperationSucceeded(created, $"Nem sikerult osszerendelni a(z) {ingredient.ProductName ?? ingredient.ProductBvin} termeket a recept kategoriaval.");
            }
        }

        public BundleSyncReference UpsertBundle(SaveRecipeRequest request, string categoryBvin)
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                return _fallbackStore.UpsertBundle(request, categoryBvin);
            }

            var catalogServices = GetRequiredProperty(hccApp, "CatalogServices");
            var products = GetRequiredProperty(catalogServices, "Products");
            var productType = GetRequiredType("Hotcakes.Commerce.Catalog.Product");

            var bundleProduct = FindProduct(products, request.BundleBvin);
            var isNewBundle = bundleProduct == null;
            if (isNewBundle)
            {
                bundleProduct = Activator.CreateInstance(productType);
            }

            ApplyBundleValues(bundleProduct, request);

            if (isNewBundle)
            {
                var created = TryInvokeAny(catalogServices, new[] { "ProductsCreateWithInventory" }, bundleProduct, true)
                    ?? TryInvokeAny(products, new[] { "Create" }, bundleProduct);
                EnsureOperationSucceeded(created, "A bundle termek letrehozasa nem sikerult.");
            }
            else
            {
                var updated = TryInvokeAny(catalogServices, new[] { "UpdateProductWithInventory", "ProductUpdate" }, bundleProduct, true)
                    ?? TryInvokeAny(products, new[] { "Update" }, bundleProduct);
                EnsureOperationSucceeded(updated, "A bundle termek frissitese nem sikerult.");
            }

            var bundleBvin = Convert.ToString(GetRequiredProperty(bundleProduct, "Bvin"));
            RebuildBundledProducts(catalogServices, bundleBvin, request.Ingredients);
            EnsureBundleCategoryRelation(catalogServices, categoryBvin, bundleBvin);

            return new BundleSyncReference
            {
                BundleBvin = bundleBvin
            };
        }

        public void Publish(string categoryBvin, string bundleBvin)
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                _fallbackStore.Publish(categoryBvin, bundleBvin);
                return;
            }

            SetCategoryVisibility(hccApp, categoryBvin, false);
            SetBundleStatus(hccApp, bundleBvin, "Active");
        }

        public void Revoke(string categoryBvin, string bundleBvin)
        {
            if (!TryGetHotcakesApp(out var hccApp))
            {
                _fallbackStore.Revoke(categoryBvin, bundleBvin);
                return;
            }

            SetCategoryVisibility(hccApp, categoryBvin, true);
            SetBundleStatus(hccApp, bundleBvin, "Disabled");
        }

        private static object FindCategory(object categories, SaveRecipeRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.CategoryBvin))
            {
                var existing = TryInvokeAny(categories, new[] { "Find", "FindWithCache" }, request.CategoryBvin);
                if (existing != null)
                {
                    return existing;
                }
            }

            var slug = BuildSlug(request.RecipeName);
            return TryInvokeAny(categories, new[] { "FindBySlug" }, slug);
        }

        private static object FindProduct(object products, string bvin)
        {
            if (string.IsNullOrWhiteSpace(bvin))
            {
                return null;
            }

            return TryInvokeAny(products, new[] { "Find", "FindWithCache" }, bvin);
        }

        private static void ApplyCategoryValues(object category, SaveRecipeRequest request)
        {
            var hidden = !(request.PublishAfterSave || string.Equals(request.Status, "Published", StringComparison.OrdinalIgnoreCase));

            SetPropertyIfPresent(category, "Name", request.RecipeName);
            SetPropertyIfPresent(category, "Description", request.Description);
            SetPropertyIfPresent(category, "MetaDescription", request.ShortDescription ?? request.Description);
            SetPropertyIfPresent(category, "RewriteUrl", BuildSlug(request.RecipeName));
            SetPropertyIfPresent(category, "Hidden", hidden);
            SetPropertyIfPresent(category, "ShowInTopMenu", false);
        }

        private static void ApplyBundleValues(object bundleProduct, SaveRecipeRequest request)
        {
            var firstIngredient = request.Ingredients.FirstOrDefault();
            var skuBase = string.IsNullOrWhiteSpace(firstIngredient?.ProductBvin)
                ? BuildSlug(request.RecipeName).ToUpperInvariant()
                : firstIngredient.ProductBvin.ToUpperInvariant();
            var bundleStatus = request.PublishAfterSave || string.Equals(request.Status, "Published", StringComparison.OrdinalIgnoreCase)
                ? "Active"
                : "Disabled";

            SetPropertyIfPresent(bundleProduct, "Sku", $"REC-{skuBase}-BUNDLE");
            SetPropertyIfPresent(bundleProduct, "ProductName", request.RecipeName);
            SetPropertyIfPresent(bundleProduct, "ShortDescription", request.ShortDescription ?? request.Description);
            SetPropertyIfPresent(bundleProduct, "LongDescription", request.Description);
            SetPropertyIfPresent(bundleProduct, "SitePrice", request.EstimatedCost ?? 0m);
            SetPropertyIfPresent(bundleProduct, "ListPrice", request.EstimatedCost ?? 0m);
            SetPropertyIfPresent(bundleProduct, "UrlSlug", $"{BuildSlug(request.RecipeName)}-bundle");
            SetPropertyIfPresent(bundleProduct, "IsBundle", true);
            SetPropertyIfPresent(bundleProduct, "IsSearchable", true);
            SetPropertyIfPresent(bundleProduct, "IsAvailableForSale", true);
            SetEnumPropertyIfPresent(bundleProduct, "Status", bundleStatus);
            SetEnumPropertyIfPresent(bundleProduct, "InventoryMode", "AlwaysInStock", "AlwayInStock");
        }

        private static void RebuildBundledProducts(object catalogServices, string bundleBvin, IEnumerable<RecipeIngredientDto> ingredients)
        {
            DeleteBundledProducts(catalogServices, bundleBvin);

            var bundledProductType = GetRequiredType("Hotcakes.Commerce.Catalog.BundledProduct");
            foreach (var ingredient in ingredients.OrderBy(i => i.SortOrder))
            {
                var bundledProduct = Activator.CreateInstance(bundledProductType);
                SetPropertyIfPresent(bundledProduct, "ProductId", bundleBvin);
                SetPropertyIfPresent(bundledProduct, "BundledProductId", ingredient.ProductBvin);
                SetPropertyIfPresent(bundledProduct, "Quantity", ingredient.Quantity);

                var created = TryInvokeAny(catalogServices, new[] { "BundledProductCreate" }, bundledProduct)
                    ?? TryInvokeAny(GetPropertyIfPresent(catalogServices, "BundledProducts"), new[] { "Create", "Add" }, bundledProduct);
                EnsureOperationSucceeded(created, $"A bundle elem letrehozasa nem sikerult: {ingredient.ProductName ?? ingredient.ProductBvin}");
            }
        }

        private static void DeleteBundledProducts(object catalogServices, string bundleBvin)
        {
            if (TryInvokeAny(catalogServices, new[] { "BundledProductsDeleteForProduct", "BundledProductDeleteForProduct" }, bundleBvin) != null)
            {
                return;
            }

            var bundledProducts = GetPropertyIfPresent(catalogServices, "BundledProducts");
            if (bundledProducts == null)
            {
                return;
            }

            if (TryInvokeAny(bundledProducts, new[] { "DeleteAllForProduct" }, bundleBvin) != null)
            {
                return;
            }

            var existing = TryInvokeAny(bundledProducts, new[] { "FindForProduct" }, bundleBvin) as IEnumerable;
            if (existing == null)
            {
                return;
            }

            foreach (var relation in existing)
            {
                var deleted = TryInvokeAny(bundledProducts, new[] { "Delete" }, relation);
                EnsureOperationSucceeded(deleted, "A korabbi bundle elemek torlese nem sikerult.");
            }
        }

        private static void EnsureBundleCategoryRelation(object catalogServices, string categoryBvin, string bundleBvin)
        {
            var relations = GetPropertyIfPresent(catalogServices, "CategoriesXProducts");
            if (relations == null)
            {
                return;
            }

            var existing = TryInvokeAny(relations, new[] { "FindByCategoryAndProduct" }, categoryBvin, bundleBvin);
            if (existing != null)
            {
                return;
            }

            var associationType = GetRequiredType("Hotcakes.Commerce.Catalog.CategoryProductAssociation");
            var association = Activator.CreateInstance(associationType);
            SetPropertyIfPresent(association, "CategoryId", categoryBvin);
            SetPropertyIfPresent(association, "ProductId", bundleBvin);
            SetPropertyIfPresent(association, "SortOrder", 0);
            TryInvokeAny(relations, new[] { "Create", "Add" }, association);
        }

        private static void SetCategoryVisibility(object hccApp, string categoryBvin, bool hidden)
        {
            if (string.IsNullOrWhiteSpace(categoryBvin))
            {
                return;
            }

            var catalogServices = GetRequiredProperty(hccApp, "CatalogServices");
            var categories = GetRequiredProperty(catalogServices, "Categories");
            var category = TryInvokeAny(categories, new[] { "Find", "FindWithCache" }, categoryBvin);
            if (category == null)
            {
                return;
            }

            SetPropertyIfPresent(category, "Hidden", hidden);
            var updated = TryInvokeAny(catalogServices, new[] { "CategoryUpdate", "UpdateCategory" }, category)
                ?? TryInvokeAny(categories, new[] { "Update" }, category);
            EnsureOperationSucceeded(updated, "A kategoria lathatosaganak frissitese nem sikerult.");
        }

        private static void SetBundleStatus(object hccApp, string bundleBvin, string statusName)
        {
            if (string.IsNullOrWhiteSpace(bundleBvin))
            {
                return;
            }

            var catalogServices = GetRequiredProperty(hccApp, "CatalogServices");
            var products = GetRequiredProperty(catalogServices, "Products");
            var product = FindProduct(products, bundleBvin);
            if (product == null)
            {
                return;
            }

            SetEnumPropertyIfPresent(product, "Status", statusName);
            SetPropertyIfPresent(product, "IsAvailableForSale", string.Equals(statusName, "Active", StringComparison.OrdinalIgnoreCase));

            var updated = TryInvokeAny(catalogServices, new[] { "UpdateProductWithInventory", "ProductUpdate" }, product, true)
                ?? TryInvokeAny(products, new[] { "Update" }, product);
            EnsureOperationSucceeded(updated, "A bundle allapotanak frissitese nem sikerult.");
        }

        private static void DeleteCategoryAssociations(object relations, string categoryBvin)
        {
            var existing = TryInvokeAny(relations, new[] { "FindForCategory" }, categoryBvin, 1, 500) as IEnumerable;
            if (existing == null)
            {
                return;
            }

            foreach (var relation in existing)
            {
                var deleted = TryInvokeAny(relations, new[] { "Delete" }, relation);
                if (deleted == null)
                {
                    var productId = Convert.ToString(GetPropertyIfPresent(relation, "ProductId"));
                    deleted = TryInvokeAny(relations, new[] { "DeleteByCategoryAndProduct" }, categoryBvin, productId);
                }

                EnsureOperationSucceeded(deleted, "A kategoria-termek kapcsolatok torlese nem sikerult.");
            }
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

                var instanceProperty = factoryType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                instanceProperty?.SetValue(null, factoryInstance, null);

                var httpContextProperty = factoryType.GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Static);
                if (HttpContext.Current != null)
                {
                    httpContextProperty?.SetValue(null, HttpContext.Current, null);
                }

                var requestContextType = GetRequiredType("Hotcakes.Commerce.HccRequestContext");
                var cultureCode = portalSettings.CultureCode ?? portalSettings.DefaultLanguage ?? "en-US";
                var requestContext = Activator.CreateInstance(requestContextType, new object[] { cultureCode });
                var resolvedStore = ResolveCurrentStore(requestContext, portalSettings);
                if (resolvedStore != null)
                {
                    SetPropertyIfPresent(requestContext, "CurrentStore", resolvedStore);
                }

                var routingContextProperty = requestContextType.GetProperty("RoutingContext", BindingFlags.Public | BindingFlags.Instance);
                if (HttpContext.Current != null)
                {
                    var routeContext = new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData());
                    routingContextProperty?.SetValue(requestContext, routeContext, null);
                }

                var currentContextProperty = requestContextType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static);
                currentContextProperty?.SetValue(null, requestContext, null);

                var contextUtilsType = GetRequiredType("Hotcakes.Commerce.Utilities.HccRequestContextUtils");
                var updateContextStore = contextUtilsType.GetMethod("UpdateContextStore", BindingFlags.Public | BindingFlags.Static);
                updateContextStore?.Invoke(null, new[] { requestContext });

                var appType = GetRequiredType("Hotcakes.Commerce.HotcakesApplication");
                hccApp = Activator.CreateInstance(appType, new[] { requestContext });
                if (hccApp == null)
                {
                    return false;
                }

                if (resolvedStore != null)
                {
                    SetPropertyIfPresent(hccApp, "CurrentStore", resolvedStore);
                }

                var updateCurrentStore = appType.GetMethod("UpdateCurrentStore", BindingFlags.Public | BindingFlags.Instance);
                updateCurrentStore?.Invoke(hccApp, null);

                var currentStore = GetPropertyIfPresent(hccApp, "CurrentStore") ?? GetPropertyIfPresent(requestContext, "CurrentStore");
                return currentStore != null;
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
                if (storeRepository == null)
                {
                    return null;
                }

                var hostName = GetCurrentHostName(portalSettings);
                if (!string.IsNullOrWhiteSpace(hostName))
                {
                    var storeId = TryInvokeAny(storeRepository, new[] { "FindStoreIdByCustomUrl" }, hostName);
                    if (TryConvertToLong(storeId, out var resolvedStoreId) && resolvedStoreId > 0)
                    {
                        var store = TryInvokeAny(storeRepository, new[] { "FindById", "FindByIdWithCache" }, resolvedStoreId);
                        if (store != null)
                        {
                            return store;
                        }
                    }
                }

                var stores = TryInvokeAny(storeRepository, new[] { "ListAllForSuper" }) as IEnumerable;
                if (stores == null)
                {
                    return null;
                }

                object firstRealStore = null;
                var count = 0;
                foreach (var store in stores)
                {
                    if (store == null)
                    {
                        continue;
                    }

                    if (!TryConvertToLong(GetPropertyIfPresent(store, "Id"), out var storeId) || storeId <= 0)
                    {
                        continue;
                    }

                    count++;
                    if (firstRealStore == null)
                    {
                        firstRealStore = store;
                    }

                    if (count > 1)
                    {
                        break;
                    }
                }

                return count == 1 ? firstRealStore : null;
            }
            catch
            {
                return null;
            }
        }

        private static string GetCurrentHostName(PortalSettings portalSettings)
        {
            var hostName = HttpContext.Current?.Request?.Url?.Host;
            if (!string.IsNullOrWhiteSpace(hostName))
            {
                return hostName;
            }

            var alias = portalSettings?.PortalAlias?.HTTPAlias;
            if (string.IsNullOrWhiteSpace(alias))
            {
                return null;
            }

            var normalized = alias;
            var schemeIndex = normalized.IndexOf("://", StringComparison.Ordinal);
            if (schemeIndex >= 0)
            {
                normalized = normalized.Substring(schemeIndex + 3);
            }

            var slashIndex = normalized.IndexOf('/');
            if (slashIndex >= 0)
            {
                normalized = normalized.Substring(0, slashIndex);
            }

            var colonIndex = normalized.IndexOf(':');
            if (colonIndex >= 0)
            {
                normalized = normalized.Substring(0, colonIndex);
            }

            return normalized;
        }

        private static bool TryConvertToLong(object value, out long result)
        {
            if (value == null)
            {
                result = 0;
                return false;
            }

            try
            {
                result = Convert.ToInt64(value);
                return true;
            }
            catch
            {
                result = 0;
                return false;
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

        private static Type GetRequiredType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .FirstOrDefault(t => string.Equals(t.FullName, fullName, StringComparison.Ordinal));
            if (type == null)
            {
                throw new InvalidOperationException($"A szukseges Hotcakes tipus nem talalhato: {fullName}");
            }

            return type;
        }

        private static object GetRequiredProperty(object target, string propertyName)
        {
            var value = GetPropertyIfPresent(target, propertyName);
            if (value == null)
            {
                throw new InvalidOperationException($"Hianyzo property: {propertyName}");
            }

            return value;
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

        private static void SetEnumPropertyIfPresent(object target, string propertyName, params string[] enumNames)
        {
            if (target == null)
            {
                return;
            }

            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
            {
                return;
            }

            foreach (var enumName in enumNames)
            {
                if (string.IsNullOrWhiteSpace(enumName))
                {
                    continue;
                }

                try
                {
                    var enumValue = Enum.Parse(property.PropertyType, enumName, true);
                    property.SetValue(target, enumValue, null);
                    return;
                }
                catch
                {
                }
            }
        }

        private static object ConvertValue(object value, Type destinationType)
        {
            if (value == null)
            {
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            var targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            if (targetType == typeof(string))
            {
                return Convert.ToString(value);
            }

            if (targetType == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }

            if (targetType == typeof(int))
            {
                return Convert.ToInt32(value);
            }

            if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }

            return Convert.ChangeType(value, targetType);
        }

        private static object InvokeAny(object target, IReadOnlyCollection<string> methodNames, params object[] args)
        {
            var result = TryInvokeAny(target, methodNames, args);
            if (result == null)
            {
                throw new MissingMethodException($"Nem talalhato megfelelo metodus: {string.Join(", ", methodNames)}");
            }

            return result;
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
                        converted[i] = ConvertMethodArgument(args[i], parameters[i].ParameterType);
                    }

                    return method.Invoke(target, converted);
                }
                catch
                {
                }
            }

            return null;
        }

        private static object ConvertMethodArgument(object value, Type parameterType)
        {
            if (value == null)
            {
                return null;
            }

            var targetType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, Convert.ToString(value), true);
            }

            if (targetType == typeof(string))
            {
                return Convert.ToString(value);
            }

            if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }

            if (targetType == typeof(int))
            {
                return Convert.ToInt32(value);
            }

            if (targetType == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }

            return Convert.ChangeType(value, targetType);
        }

        private static void EnsureOperationSucceeded(object result, string message)
        {
            if (result == null)
            {
                return;
            }

            if (result is bool boolResult && !boolResult)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string BuildSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Guid.NewGuid().ToString("N");
            }

            var cleaned = new string(value
                .Trim()
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray());

            while (cleaned.Contains("--"))
            {
                cleaned = cleaned.Replace("--", "-");
            }

            return cleaned.Trim('-');
        }

        private sealed class FallbackStore
        {
            private readonly string _filePath;
            private readonly object _syncRoot = new object();

            public FallbackStore()
            {
                var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
                Directory.CreateDirectory(appData);
                _filePath = Path.Combine(appData, "NaturaCo.RecipeSync.fallback.json");
            }

            public CategorySyncReference UpsertRecipeCategory(SaveRecipeRequest request)
            {
                lock (_syncRoot)
                {
                    var state = Load();
                    var category = state.Categories.FirstOrDefault(c =>
                        string.Equals(c.CategoryBvin, request.CategoryBvin, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Slug, BuildSlug(request.RecipeName), StringComparison.OrdinalIgnoreCase));

                    if (category == null)
                    {
                        category = new FallbackCategory
                        {
                            CategoryBvin = Guid.NewGuid().ToString("N"),
                            Slug = BuildSlug(request.RecipeName)
                        };
                        state.Categories.Add(category);
                    }

                    category.RecipeName = request.RecipeName;
                    category.Status = request.PublishAfterSave ? "Published" : request.Status ?? "Draft";
                    Save(state);

                    return new CategorySyncReference
                    {
                        CategoryBvin = category.CategoryBvin
                    };
                }
            }

            public void ReplaceCategoryProducts(string categoryBvin, IReadOnlyCollection<RecipeIngredientDto> ingredients)
            {
                lock (_syncRoot)
                {
                    var state = Load();
                    var category = state.Categories.FirstOrDefault(c => c.CategoryBvin == categoryBvin);
                    if (category == null)
                    {
                        return;
                    }

                    category.Products = ingredients
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new FallbackProductLink
                        {
                            ProductBvin = i.ProductBvin,
                            Quantity = i.Quantity,
                            Unit = i.Unit
                        })
                        .ToList();

                    Save(state);
                }
            }

            public BundleSyncReference UpsertBundle(SaveRecipeRequest request, string categoryBvin)
            {
                lock (_syncRoot)
                {
                    var state = Load();
                    var bundle = state.Bundles.FirstOrDefault(b =>
                        string.Equals(b.BundleBvin, request.BundleBvin, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(b.CategoryBvin, categoryBvin, StringComparison.OrdinalIgnoreCase));

                    if (bundle == null)
                    {
                        bundle = new FallbackBundle
                        {
                            BundleBvin = Guid.NewGuid().ToString("N")
                        };
                        state.Bundles.Add(bundle);
                    }

                    bundle.CategoryBvin = categoryBvin;
                    bundle.RecipeName = request.RecipeName;
                    bundle.Status = request.PublishAfterSave ? "Published" : request.Status ?? "Draft";
                    bundle.Items = request.Ingredients
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new FallbackProductLink
                        {
                            ProductBvin = i.ProductBvin,
                            Quantity = i.Quantity,
                            Unit = i.Unit
                        })
                        .ToList();

                    Save(state);

                    return new BundleSyncReference
                    {
                        BundleBvin = bundle.BundleBvin
                    };
                }
            }

            public void Publish(string categoryBvin, string bundleBvin)
            {
                lock (_syncRoot)
                {
                    var state = Load();
                    UpdateStatus(state, categoryBvin, bundleBvin, "Published");
                    Save(state);
                }
            }

            public void Revoke(string categoryBvin, string bundleBvin)
            {
                lock (_syncRoot)
                {
                    var state = Load();
                    UpdateStatus(state, categoryBvin, bundleBvin, "Revoked");
                    Save(state);
                }
            }

            private static void UpdateStatus(FallbackState state, string categoryBvin, string bundleBvin, string status)
            {
                var category = state.Categories.FirstOrDefault(c => c.CategoryBvin == categoryBvin);
                if (category != null)
                {
                    category.Status = status;
                }

                var bundle = state.Bundles.FirstOrDefault(b => b.BundleBvin == bundleBvin || b.CategoryBvin == categoryBvin);
                if (bundle != null)
                {
                    bundle.Status = status;
                }
            }

            private FallbackState Load()
            {
                if (!File.Exists(_filePath))
                {
                    return new FallbackState();
                }

                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Length == 0)
                {
                    return new FallbackState();
                }

                try
                {
                    using var stream = File.OpenRead(_filePath);
                    var serializer = new DataContractJsonSerializer(typeof(FallbackState));
                    return serializer.ReadObject(stream) as FallbackState ?? new FallbackState();
                }
                catch
                {
                    return new FallbackState();
                }
            }

            private void Save(FallbackState state)
            {
                using var stream = File.Create(_filePath);
                var serializer = new DataContractJsonSerializer(typeof(FallbackState));
                serializer.WriteObject(stream, state);
            }
        }

        [DataContract]
        public sealed class FallbackState
        {
            [DataMember]
            public List<FallbackCategory> Categories { get; set; } = new List<FallbackCategory>();

            [DataMember]
            public List<FallbackBundle> Bundles { get; set; } = new List<FallbackBundle>();
        }

        [DataContract]
        public sealed class FallbackCategory
        {
            [DataMember]
            public string CategoryBvin { get; set; }

            [DataMember]
            public string Slug { get; set; }

            [DataMember]
            public string RecipeName { get; set; }

            [DataMember]
            public string Status { get; set; }

            [DataMember]
            public List<FallbackProductLink> Products { get; set; } = new List<FallbackProductLink>();
        }

        [DataContract]
        public sealed class FallbackBundle
        {
            [DataMember]
            public string BundleBvin { get; set; }

            [DataMember]
            public string CategoryBvin { get; set; }

            [DataMember]
            public string RecipeName { get; set; }

            [DataMember]
            public string Status { get; set; }

            [DataMember]
            public List<FallbackProductLink> Items { get; set; } = new List<FallbackProductLink>();
        }

        [DataContract]
        public sealed class FallbackProductLink
        {
            [DataMember]
            public string ProductBvin { get; set; }

            [DataMember]
            public decimal Quantity { get; set; }

            [DataMember]
            public string Unit { get; set; }
        }
    }
}
