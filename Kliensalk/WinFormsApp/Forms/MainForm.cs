using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NaturaCo.RecipeEditor.Models;
using NaturaCo.RecipeEditor.Services;

namespace NaturaCo.RecipeEditor.Forms
{
    public partial class MainForm : Form
    {
        private readonly HotCakesService  _hccService;
        private readonly RecipeApiService _recipeService;

        // Editor-allapot. A szerver-szintu "igazsagot" a CategoryBvin / BundleBvin
        // hordozza - ezeket minden sikeres Save utan frissitjuk
        // (CLIENT_APP_CONTEXT.md - "Mi a kliens szamara a legfontosabb Save utan").
        private Recipe            _currentRecipe;
        private List<HccProduct>  _allProducts = new List<HccProduct>();
        private string            _currentCategoryBvin;
        private bool              _suppressRecipeSelect;
        // CategoryBvin mint kulcs: a szerver Save valasz mindig tartalmazza,
        // RecipeId-t nem mindig (lehet null/0). Igy a cache biztosan muKodik.
        private readonly Dictionary<string, Recipe> _recipeCache =
            new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);

        // Ezen ket Hotcakes kategoriaban gramm az alapertelmezett mertekegyseg,
        // mert a termekekhez strukturaltan jar Csomag: option ("500 g", "1 kg" stb.).
        private static readonly HashSet<string> GramCategoryBvins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ad392bb1-bf4a-4f0b-9a3b-f87e878000a1", // Feherjeforras
            "a42772d0-d5eb-430d-b101-306a1fd0dc17"  // Rost es vitamin
        };

        public MainForm(HotCakesService hccService, RecipeApiService recipeService)
        {
            InitializeComponent();
            _hccService    = hccService;
            _recipeService = recipeService;

            SetupIngredientGrid();
            SetupRecipeList();
        }

        private void SetupRecipeList()
        {
            lvRecipes.Columns.Clear();
            lvRecipes.Columns.Add("Recept neve", 185);
            lvRecipes.Columns.Add("Állapot",      79);
        }

        // A dgvIngredients csak a 4 szerkesztoi mezot mutatja
        // (Hozzavalo / Mennyiseg / Mertekegyseg / Sorrend), de a kotott
        // RecipeIngredient peldanyokon a ProductBvin es a LinkedProduct*
        // mezok ervintetlenul tarolodnak - a Save oket onnan olvassa ki.
        private void SetupIngredientGrid()
        {
            dgvIngredients.AutoGenerateColumns = false;
            dgvIngredients.Columns.Clear();

            dgvIngredients.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "colIngredientName",
                HeaderText       = "Hozzavalo",
                DataPropertyName = nameof(RecipeIngredient.IngredientName),
                AutoSizeMode     = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvIngredients.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "colAmount",
                HeaderText       = "Mennyiseg",
                DataPropertyName = nameof(RecipeIngredient.Amount),
                Width            = 100
            });

            dgvIngredients.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "colUnit",
                HeaderText       = "Mertekegyseg",
                DataPropertyName = nameof(RecipeIngredient.Unit),
                Width            = 110
            });

            dgvIngredients.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name             = "colSortOrder",
                HeaderText       = "Sorrend",
                DataPropertyName = nameof(RecipeIngredient.SortOrder),
                Width            = 80
            });

            // A grid sorbeli szerkesztes utan (Amount, Unit) ujraszamolunk
            dgvIngredients.CellValueChanged += (s, e) => { if (e.RowIndex >= 0) RecalculateTotals(); };
            dgvIngredients.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvIngredients.IsCurrentCellDirty)
                    dgvIngredients.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        // ------------------------------------------------------------------
        // Eletciklus
        // ------------------------------------------------------------------

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
            NewRecipe();
            await LoadRecipeListAsync();
        }

        private Task LoadCategoriesAsync()
        {
            try
            {
                var cats = _hccService.GetCategories();
                cmbCategory.DataSource    = cats;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember   = "Bvin";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategoriak betoltese sikertelen: " + ex.Message);
            }
            return Task.CompletedTask;
        }

        // ------------------------------------------------------------------
        // Recept lista (jobb oldali panel)
        // ------------------------------------------------------------------

        private async Task LoadRecipeListAsync()
        {
            _suppressRecipeSelect = true;
            try
            {
                // Eloszor a szerver /List vegpontot probaljuk
                var items = await _recipeService.GetRecipesAsync();

                if (items != null && items.Count > 0)
                {
                    lvRecipes.Items.Clear();
                    foreach (var r in items)
                    {
                        var lvi = new ListViewItem(r.RecipeName ?? "(névtelen)") { Tag = r };
                        lvi.SubItems.Add(StatusToHu(r.Status));
                        lvRecipes.Items.Add(lvi);
                    }
                    return;
                }

                // Fallback: /List vegpont nem letezik meg -> HotCakes hidden kategoriak
                var hccItems = _hccService.GetAllRecipesFromHcc();
                lvRecipes.Items.Clear();
                foreach (var r in hccItems)
                {
                    // Ha a helyi listaban mar szerepel (SyncRecipeInList tette oda),
                    // attol az allapotot es RecipeId-t vesszuk at
                    var existing = FindInList(r.CategoryBvin);
                    if (existing != null) r.Status = existing.Status;

                    var lvi = new ListViewItem(r.RecipeName ?? "(névtelen)") { Tag = r };
                    lvi.SubItems.Add(StatusToHu(r.Status));
                    lvRecipes.Items.Add(lvi);
                }
            }
            catch { /* lista marad uresen */ }
            finally
            {
                _suppressRecipeSelect = false;
            }
        }

        private RecipeListItem FindInList(string categoryBvin)
        {
            foreach (ListViewItem lvi in lvRecipes.Items)
                if (lvi.Tag is RecipeListItem li && li.CategoryBvin == categoryBvin)
                    return li;
            return null;
        }

        // Mentés/publikálás után azonnal frissíti a listát a szerver vegponttol
        // fuggetlenul (ha a /List vegpont nem letezik meg, a helyi listaban is latszik).
        private void SyncRecipeInList(Recipe r)
        {
            _suppressRecipeSelect = true;
            try
            {
                foreach (ListViewItem lvi in lvRecipes.Items)
                {
                    if (lvi.Tag is RecipeListItem li && li.RecipeId == r.RecipeID)
                    {
                        lvi.Text = r.RecipeName ?? "(névtelen)";
                        lvi.SubItems[1].Text = StatusToHu(r.Status);
                        return;
                    }
                }

                var newLvi = new ListViewItem(r.RecipeName ?? "(névtelen)")
                {
                    Tag = new RecipeListItem
                    {
                        RecipeId     = r.RecipeID,
                        RecipeName   = r.RecipeName,
                        Status       = r.Status,
                        CategoryBvin = r.CategoryBvin,
                        BundleBvin   = r.BundleBvin
                    }
                };
                newLvi.SubItems.Add(StatusToHu(r.Status));
                lvRecipes.Items.Add(newLvi);
            }
            finally
            {
                _suppressRecipeSelect = false;
            }
        }

        private void SelectMealType(string category)
        {
            if (string.IsNullOrEmpty(category)) { cmbMealType.SelectedIndex = -1; return; }
            for (int i = 0; i < cmbMealType.Items.Count; i++)
            {
                if (string.Equals(cmbMealType.Items[i].ToString(), category, StringComparison.OrdinalIgnoreCase))
                { cmbMealType.SelectedIndex = i; return; }
            }
            cmbMealType.SelectedIndex = -1;
        }

        private static string StatusToHu(string status) => status switch
        {
            "Published" => "Publikált",
            "Revoked"   => "Visszavont",
            _           => "Tervezet"
        };

        private async void lvRecipes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressRecipeSelect) return;
            if (lvRecipes.SelectedItems.Count == 0) return;
            if (!(lvRecipes.SelectedItems[0].Tag is RecipeListItem item)) return;

            try
            {
                SetBusy(true);

                // 1. Helyi cache (CategoryBvin kulcs) - mentett receptek teljes adattal
                if (!string.IsNullOrEmpty(item.CategoryBvin) &&
                    _recipeCache.TryGetValue(item.CategoryBvin, out var cached))
                {
                    LoadRecipeIntoForm(cached);
                    return;
                }

                // 2. Szerver /Load vegpont
                if (item.RecipeId > 0)
                {
                    var result = await _recipeService.LoadRecipeAsync(item.RecipeId);
                    if (result != null && result.Success)
                    {
                        PopulateFormFromResult(result);
                        await EnrichIngredientsFromHccAsync();
                        return;
                    }
                }

                // 3. Fallback: csak a lista adatai (HCC-alapu elem, vagy hianyzo vegpont)
                PopulateFormFromListItem(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Betoltesi hiba: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Mentes utan az aktualis receptet elmentjuk a helyi cache-be,
        // hogy visszakatintaskor minden mezo latsszon a szerver /Load nelkul is.
        private void CacheCurrentRecipe()
        {
            if (_currentRecipe == null || string.IsNullOrEmpty(_currentRecipe.CategoryBvin)) return;
            _recipeCache[_currentRecipe.CategoryBvin] = new Recipe
            {
                RecipeID        = _currentRecipe.RecipeID,
                RecipeName      = _currentRecipe.RecipeName,
                Description     = _currentRecipe.Description,
                Category        = _currentRecipe.Category,
                ShortDescription= _currentRecipe.ShortDescription,
                Steps           = _currentRecipe.Steps,
                Servings        = _currentRecipe.Servings,
                PrepTimeMinutes = _currentRecipe.PrepTimeMinutes,
                CookTimeMinutes = _currentRecipe.CookTimeMinutes,
                TotalCalories   = _currentRecipe.TotalCalories,
                EstimatedCost   = _currentRecipe.EstimatedCost,
                Status          = _currentRecipe.Status,
                CategoryBvin    = _currentRecipe.CategoryBvin,
                BundleBvin      = _currentRecipe.BundleBvin,
                Ingredients     = _currentRecipe.Ingredients.Select(i => new RecipeIngredient
                {
                    IngredientName     = i.IngredientName,
                    Amount             = i.Amount,
                    Unit               = i.Unit,
                    ProductBvin        = i.ProductBvin,
                    LinkedProductName  = i.LinkedProductName,
                    LinkedProductPrice = i.LinkedProductPrice,
                    PricePerGram       = i.PricePerGram,
                    CaloriesPer100g    = i.CaloriesPer100g,
                    PackageQuantity    = i.PackageQuantity,
                    PackageUnit        = i.PackageUnit,
                    SortOrder          = i.SortOrder
                }).ToList()
            };
        }

        private void LoadRecipeIntoForm(Recipe r)
        {
            _currentRecipe = r;
            txtRecipeName.Text  = r.RecipeName      ?? string.Empty;
            txtDescription.Text = r.Description     ?? string.Empty;
            txtSteps.Text       = r.Steps           ?? string.Empty;
            SelectMealType(r.Category);
            nudServings.Value   = Math.Max(nudServings.Minimum, Math.Min(nudServings.Maximum, r.Servings > 0 ? r.Servings : 1));
            nudPrepTime.Value   = Math.Max(0, Math.Min(nudPrepTime.Maximum, r.PrepTimeMinutes));
            nudCookTime.Value   = Math.Max(0, Math.Min(nudCookTime.Maximum, r.CookTimeMinutes));
            SetStatus(r.Status);
            RefreshIngredientGrid();
            RecalculateTotals();
        }

        private void PopulateFormFromListItem(RecipeListItem item)
        {
            _currentRecipe = new Recipe
            {
                RecipeID     = item.RecipeId,
                RecipeName   = item.RecipeName   ?? string.Empty,
                Status       = item.Status == "?" ? "Draft" : (item.Status ?? "Draft"),
                CategoryBvin = item.CategoryBvin ?? string.Empty,
                BundleBvin   = item.BundleBvin   ?? string.Empty
            };

            txtRecipeName.Text  = _currentRecipe.RecipeName;
            txtDescription.Text = string.Empty;
            cmbMealType.SelectedIndex = -1;
            txtSteps.Text       = string.Empty;
            nudServings.Value   = 4;
            nudPrepTime.Value   = 0;
            nudCookTime.Value   = 0;

            SetStatus(_currentRecipe.Status);
            RefreshIngredientGrid();
            RecalculateTotals();
        }

        private void PopulateFormFromResult(RecipeLoadResult r)
        {
            LoadRecipeIntoForm(new Recipe
            {
                RecipeID         = r.RecipeId,
                RecipeName       = r.RecipeName       ?? string.Empty,
                ShortDescription = r.ShortDescription ?? string.Empty,
                Description      = r.Description      ?? string.Empty,
                Category         = r.Category         ?? string.Empty,
                Steps            = r.Steps            ?? string.Empty,
                Servings         = r.Servings > 0 ? r.Servings : 1,
                PrepTimeMinutes  = r.PrepTime,
                CookTimeMinutes  = r.CookTime,
                TotalCalories    = r.TotalCalories,
                EstimatedCost    = r.EstimatedCost,
                Status           = r.Status           ?? "Draft",
                CategoryBvin     = r.CategoryBvin      ?? string.Empty,
                BundleBvin       = r.BundleBvin        ?? string.Empty,
                Ingredients      = (r.Ingredients ?? new List<RecipeIngredientDto>())
                    .Select(i => new RecipeIngredient
                    {
                        IngredientName     = i.ProductName,
                        Amount             = i.Quantity,
                        Unit               = i.Unit,
                        ProductBvin        = i.ProductBvin,
                        SortOrder          = i.SortOrder,
                        LinkedProductPrice = i.Price,
                        CaloriesPer100g    = i.Calories,
                        PackageQuantity    = i.PackageQuantity,
                        PackageUnit        = i.PackageUnit
                    }).ToList()
            });
        }

        // Régi receptek betöltésekor feltölti az ár/kalória adatokat HotCakes-ből,
        // ahol az adatbázisban még nem volt eltárolva (Price=0, CaloriesPer100g=0).
        private async System.Threading.Tasks.Task EnrichIngredientsFromHccAsync()
        {
            if (_currentRecipe == null) return;

            var toEnrich = _currentRecipe.Ingredients
                .Where(i => !string.IsNullOrEmpty(i.ProductBvin))
                .ToList();

            if (toEnrich.Count == 0) return;

            await System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var ing in toEnrich)
                {
                    try
                    {
                        var product = _hccService.FindProduct(ing.ProductBvin);
                        if (product == null) continue;

                        ing.LinkedProductName  = product.ProductName;
                        ing.LinkedProductPrice = product.SitePrice;
                        ing.CaloriesPer100g    = product.CaloriesPer100g;
                        ing.PricePerGram       = _hccService.GetPricePerGramOrZero(product);
                    }
                    catch { /* ha egy termék nem elérhető, továbblépünk */ }
                }
            });

            RefreshIngredientGrid();
            RecalculateTotals();
        }

        private void btnNewRecipe_Click(object sender, EventArgs e)
        {
            _suppressRecipeSelect = true;
            lvRecipes.SelectedItems.Clear();
            _suppressRecipeSelect = false;

            txtRecipeName.Text  = string.Empty;
            txtDescription.Text = string.Empty;
            cmbMealType.SelectedIndex = -1;
            txtSteps.Text       = string.Empty;
            nudServings.Value   = 4;
            nudPrepTime.Value   = 0;
            nudCookTime.Value   = 0;

            NewRecipe();
        }

        // ------------------------------------------------------------------
        // Termekkatalogus -> hozzavalo
        // ------------------------------------------------------------------

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategory.SelectedValue is string bvin)
                LoadProductsForCategory(bvin);
        }

        private void LoadProductsForCategory(string categoryBvin)
        {
            try
            {
                _currentCategoryBvin      = categoryBvin;
                _allProducts              = _hccService.GetProductsByCategory(categoryBvin);
                lstProducts.DataSource    = _allProducts;
                lstProducts.DisplayMember = "ProductName";
                lstProducts.ValueMember   = "Bvin";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Termekek betoltese sikertelen: " + ex.Message);
            }
        }

        private bool IsGramCategory(string categoryBvin)
        {
            return !string.IsNullOrEmpty(categoryBvin)
                && GramCategoryBvins.Contains(categoryBvin);
        }

        private void lstProducts_DoubleClick(object sender, EventArgs e)
        {
            if (lstProducts.SelectedItem is HccProduct product)
                AddIngredient(product);
        }

        private void AddIngredient(HccProduct product)
        {
            var gramCategory  = IsGramCategory(_currentCategoryBvin);
            var pricePerGram  = gramCategory ? _hccService.GetPricePerGramOrZero(product) : 0m;

            var ingredient = new RecipeIngredient
            {
                IngredientName     = product.ProductName,
                Amount             = gramCategory ? 100m : 1m,
                Unit               = gramCategory ? "g"  : "db",
                ProductBvin        = product.Bvin,
                LinkedProductName  = product.ProductName,
                LinkedProductPrice = product.SitePrice,
                PricePerGram       = pricePerGram,
                CaloriesPer100g    = product.CaloriesPer100g,
                SortOrder          = _currentRecipe.Ingredients.Count + 1
            };

            _currentRecipe.Ingredients.Add(ingredient);
            RefreshIngredientGrid();
            RecalculateTotals();
        }

        // Kombinalt koltsegszamitas: ha gramm-alapu (Unit="g" + van Ft/g) -> Ft/g * Amount,
        // egyebkent darab-logika (Ft/db * Amount).
        private static decimal CalculateLineCost(RecipeIngredient i)
        {
            if (i.PricePerGram > 0 && string.Equals(i.Unit, "g", StringComparison.OrdinalIgnoreCase))
                return i.PricePerGram * i.Amount;

            return i.LinkedProductPrice * i.Amount;
        }

        // Unit="g": kcal = (kcal/100g) / 100 * Amount.
        // Unit barmi mas (db, ek, csipet ...): 1 db ~ 100g kozelitessel kcal/100g * Amount.
        private static decimal CalculateLineCalories(RecipeIngredient i)
        {
            if (i.CaloriesPer100g <= 0) return 0m;

            if (string.Equals(i.Unit, "g", StringComparison.OrdinalIgnoreCase))
                return i.CaloriesPer100g / 100m * i.Amount;

            return i.CaloriesPer100g * i.Amount;
        }

        private void nudServings_ValueChanged(object sender, EventArgs e) => RecalculateTotals();

        private void RecalculateTotals()
        {
            if (_currentRecipe == null) return;

            var cost     = _currentRecipe.Ingredients.Sum(CalculateLineCost);
            var calories = _currentRecipe.Ingredients.Sum(CalculateLineCalories);
            var servings = (int)nudServings.Value; // mindig a UI erteket hasznaljuk

            _currentRecipe.EstimatedCost = cost;
            _currentRecipe.Servings      = servings; // szinkronban tartjuk a modellel
            _currentRecipe.TotalCalories = calories > 0 ? (int)Math.Round(calories) : (int?)null;

            lblTotalCost.Text      = $"Becsult koltseg: {cost:N0} Ft";
            lblCostPerServing.Text = servings > 0
                ? $"Adagonkent: {cost / servings:N0} Ft"
                : "Adagonkent: -";

            lblTotalCalories.Text      = $"Ossz. kaloria: {calories:N0} kcal";
            lblCaloriesPerServing.Text = servings > 0
                ? $"Adagonkent: {calories / servings:N0} kcal"
                : "Adagonkent: -";
        }

        // ------------------------------------------------------------------
        // Save / Publish / Revoke
        // ------------------------------------------------------------------

        private async void btnSaveDraft_Click(object sender, EventArgs e)
        {
            ReadFormToRecipe();
            _currentRecipe.Status = "Draft";

            var validation = ValidateForSave(_currentRecipe);
            if (validation != null)
            {
                MessageBox.Show(validation, "Hianyzo adat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = BuildSaveRequest(_currentRecipe, publishAfterSave: false);

            try
            {
                SetBusy(true);
                var result = await _recipeService.SaveAsync(request);
                await ApplyServerResult(result, defaultSuccess: "Tervezet elmentve.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mentesi hiba: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnPublish_Click(object sender, EventArgs e)
        {
            // Publikalashoz CategoryBvin kell - kulonben a szerver nem talalja a receptet.
            if (string.IsNullOrWhiteSpace(_currentRecipe.CategoryBvin))
            {
                MessageBox.Show("Elobb mentsd el a receptet (CategoryBvin meg nincs).");
                return;
            }

            var confirm = MessageBox.Show(
                "Biztosan publikalod a receptet? Megjelenik a webshopban.",
                "Publikalas megerositese",
                MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes) return;

            var request = new PublishRecipeRequest
            {
                RecipeId     = _currentRecipe.RecipeID == 0 ? (int?)null : _currentRecipe.RecipeID,
                CategoryBvin = _currentRecipe.CategoryBvin,
                BundleBvin   = _currentRecipe.BundleBvin ?? string.Empty
            };

            try
            {
                SetBusy(true);
                var result = await _recipeService.PublishAsync(request);
                await ApplyServerResult(result, defaultSuccess: "Recept sikeresen publikalva.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Publikalasi hiba: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnRevoke_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentRecipe.CategoryBvin))
            {
                MessageBox.Show("Elobb mentsd el a receptet (CategoryBvin meg nincs).");
                return;
            }

            var confirm = MessageBox.Show(
                "Biztosan visszavonod a receptet? Eltunik a webshoprol.",
                "Visszavonas megerositese",
                MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes) return;

            var request = new RevokeRecipeRequest
            {
                RecipeId     = _currentRecipe.RecipeID == 0 ? (int?)null : _currentRecipe.RecipeID,
                CategoryBvin = _currentRecipe.CategoryBvin,
                BundleBvin   = _currentRecipe.BundleBvin ?? string.Empty
            };

            try
            {
                SetBusy(true);
                var result = await _recipeService.RevokeAsync(request);
                await ApplyServerResult(result, defaultSuccess: "Recept visszavonva.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Visszavonasi hiba: " + ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ------------------------------------------------------------------
        // Request felepitese / valasz feldolgozasa
        // ------------------------------------------------------------------

        private static SaveRecipeRequest BuildSaveRequest(Recipe r, bool publishAfterSave)
        {
            return new SaveRecipeRequest
            {
                RecipeId             = r.RecipeID == 0 ? (int?)null : r.RecipeID,
                RecipeName           = r.RecipeName,
                MealType             = r.Category ?? string.Empty,
                ShortDescription     = r.ShortDescription ?? string.Empty,
                Description          = r.Description ?? string.Empty,
                Steps                = r.Steps ?? string.Empty,
                Tags                 = r.Tags ?? string.Empty,
                Servings             = r.Servings,
                PrepTimeMinutes      = r.PrepTimeMinutes,
                CookTimeMinutes      = r.CookTimeMinutes,
                TotalCalories        = r.TotalCalories,
                EstimatedCost        = r.EstimatedCost ?? 0m,
                AuthorName           = r.AuthorName ?? string.Empty,
                PreviewImageUrl      = r.PreviewImageUrl ?? string.Empty,
                Status               = string.IsNullOrWhiteSpace(r.Status) ? "Draft" : r.Status,
                CategoryBvin         = r.CategoryBvin ?? string.Empty,
                BundleBvin           = r.BundleBvin ?? string.Empty,
                CreateOrUpdateBundle = false,
                PublishAfterSave     = publishAfterSave,
                Ingredients          = r.Ingredients.Select(i => new RecipeIngredientDto
                {
                    ProductBvin     = i.ProductBvin,
                    ProductName     = i.IngredientName,
                    Quantity        = i.Amount,
                    Unit            = i.Unit,
                    SortOrder       = i.SortOrder,
                    Calories        = CalculateLineCalories(i),
                    Price           = CalculateLineCost(i),
                    PackageQuantity = i.PackageQuantity,
                    PackageUnit     = i.PackageUnit ?? i.Unit
                }).ToList()
            };
        }

        // Klienssel azonos szabalyok, mint a server-side validacio
        // (CLIENT_APP_CONTEXT.md "Save validacio"). Igy a felhasznalo
        // korabban kap visszajelzest, mint a szerver 400-as valaszbol.
        private static string ValidateForSave(Recipe r)
        {
            if (string.IsNullOrWhiteSpace(r.RecipeName))
                return "A recept nevet ki kell tolteni.";

            if (r.Servings <= 0)
                return "Az adagok szama legyen nagyobb mint 0.";

            if (r.Ingredients == null || r.Ingredients.Count == 0)
                return "Legalabb egy hozzavalo szukseges.";

            for (int i = 0; i < r.Ingredients.Count; i++)
            {
                var ing = r.Ingredients[i];
                if (string.IsNullOrWhiteSpace(ing.ProductBvin))
                    return $"A {i + 1}. hozzavalonal hianyzik a termek (ProductBvin).";
                if (ing.Amount <= 0)
                    return $"A {i + 1}. hozzavalo mennyisege legyen nagyobb mint 0.";
            }

            return null;
        }

        // A szerver valaszat egysegesen kezeljuk:
        // - sikeres mentesnel a CategoryBvin / BundleBvin / Status visszairodik az editorba
        // - hibanal a Message + Errors lista latszik
        private async Task ApplyServerResult(RecipeSyncResult result, string defaultSuccess)
        {
            if (result == null)
            {
                MessageBox.Show("Ures szerver-valasz.");
                return;
            }

            if (result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.CategoryBvin))
                    _currentRecipe.CategoryBvin = result.CategoryBvin;

                if (!string.IsNullOrWhiteSpace(result.BundleBvin))
                    _currentRecipe.BundleBvin = result.BundleBvin;

                if (result.RecipeId.HasValue)
                    _currentRecipe.RecipeID = result.RecipeId.Value;

                if (!string.IsNullOrWhiteSpace(result.Status))
                    _currentRecipe.Status = result.Status;

                SetStatus(_currentRecipe.Status);
                CacheCurrentRecipe();
                SyncRecipeInList(_currentRecipe);
                MessageBox.Show(string.IsNullOrWhiteSpace(result.Message) ? defaultSuccess : result.Message);
                await LoadRecipeListAsync();
                return;
            }

            // Sikertelen: Message + Errors osszesitese
            var msg = string.IsNullOrWhiteSpace(result.Message) ? "A muvelet sikertelen volt." : result.Message;
            if (result.Errors != null && result.Errors.Count > 0)
                msg += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, result.Errors);

            MessageBox.Show(msg, "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ------------------------------------------------------------------
        // Form-allapot kezelese
        // ------------------------------------------------------------------

        private void NewRecipe()
        {
            _currentRecipe = new Recipe
            {
                Status   = "Draft",
                Servings = 4
            };
            RefreshIngredientGrid();
            SetStatus("Draft");
        }

        private void ReadFormToRecipe()
        {
            _currentRecipe.RecipeName       = txtRecipeName.Text.Trim();
            _currentRecipe.Description      = txtDescription.Text.Trim();
            var mealType = cmbMealType.SelectedItem?.ToString() ?? string.Empty;
            _currentRecipe.ShortDescription = mealType;
            _currentRecipe.Category         = mealType;
            _currentRecipe.AuthorName       = string.Empty;
            _currentRecipe.Tags             = string.Empty;
            _currentRecipe.Servings         = (int)nudServings.Value;
            _currentRecipe.PrepTimeMinutes  = (int)nudPrepTime.Value;
            _currentRecipe.CookTimeMinutes  = (int)nudCookTime.Value;
            _currentRecipe.Steps            = txtSteps.Text.Trim();
            // TotalCalories: a hozzavalokbol szamolt erteket hasznaljuk
            // (RecalculateTotals beallitja), kezi szerkesztes mar nincs.
        }

        private void SetStatus(string status)
        {
            cmbStatus.SelectedItem = status;
            lblStatus.Text = status switch
            {
                "Published" => "Allapot: Publikalt",
                "Revoked"   => "Allapot: Visszavont",
                _           => "Allapot: Tervezet"
            };
        }

        private void RefreshIngredientGrid()
        {
            if (_currentRecipe == null) return;
            dgvIngredients.DataSource = null;
            dgvIngredients.DataSource = _currentRecipe.Ingredients
                .OrderBy(i => i.SortOrder)
                .ToList();
        }

        private void SetBusy(bool busy)
        {
            btnSaveDraft.Enabled = !busy;
            btnPublish.Enabled   = !busy;
            btnRevoke.Enabled    = !busy;
            UseWaitCursor        = busy;
        }
    }
}
