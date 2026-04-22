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
        private readonly HotCakesService _hccService;
        private readonly RecipeApiService _recipeService;

        private Recipe          _currentRecipe;
        private List<HccProduct> _allProducts = new List<HccProduct>();

        public MainForm(HotCakesService hccService, RecipeApiService recipeService)
        {
            InitializeComponent();
            _hccService    = hccService;
            _recipeService = recipeService;
        }

        // ------------------------------------------------------------------
        // Betöltés
        // ------------------------------------------------------------------

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
            NewRecipe();
        }

        private async Task LoadCategoriesAsync()
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
                MessageBox.Show("Kategóriák betöltése sikertelen: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Termékböngésző – kategória váltáskor frissül
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
                _allProducts = _hccService.GetProductsByCategory(categoryBvin);

                lstProducts.DataSource    = _allProducts;
                lstProducts.DisplayMember = "ProductName";
                lstProducts.ValueMember   = "Bvin";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Termékek betöltése sikertelen: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Termék hozzáadása a recepthez (drag or double-click)
        // ------------------------------------------------------------------

        private void lstProducts_DoubleClick(object sender, EventArgs e)
        {
            if (lstProducts.SelectedItem is HccProduct product)
                AddIngredient(product);
        }

        private void AddIngredient(HccProduct product)
        {
            var ingredient = new RecipeIngredient
            {
                IngredientName     = product.ProductName,
                Amount             = 1,
                Unit               = "db",
                ProductID          = Guid.TryParse(product.Bvin, out var g) ? g : (Guid?)null,
                LinkedProductName  = product.ProductName,
                LinkedProductPrice = product.SitePrice,
                SortOrder          = _currentRecipe.Ingredients.Count + 1
            };

            _currentRecipe.Ingredients.Add(ingredient);
            RefreshIngredientGrid();
            RecalculateTotals();
        }

        // ------------------------------------------------------------------
        // Automatikus számítások
        // ------------------------------------------------------------------

        private void RecalculateTotals()
        {
            var cost = _currentRecipe.Ingredients
                .Where(i => i.LinkedProductPrice > 0)
                .Sum(i => i.LinkedProductPrice * i.Amount);

            _currentRecipe.EstimatedCost = cost;

            lblTotalCost.Text    = $"Becsült költség: {cost:N0} Ft";
            lblCostPerServing.Text = _currentRecipe.Servings > 0
                ? $"Adagonként: {cost / _currentRecipe.Servings:N0} Ft"
                : "";

            // Tápértékek összesítése itt bővíthető, ha a termékeken
            // custom property-ként tárolják a makrókat
        }

        // ------------------------------------------------------------------
        // Mentés tervezetként
        // ------------------------------------------------------------------

        private async void btnSaveDraft_Click(object sender, EventArgs e)
        {
            ReadFormToRecipe();
            _currentRecipe.Status = "Draft";

            try
            {
                _currentRecipe = await _recipeService.SaveRecipeAsync(_currentRecipe);
                UpdateStatusPills(_currentRecipe.Status);
                MessageBox.Show("Tervezet elmentve.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mentési hiba: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Publikálás
        // ------------------------------------------------------------------

        private async void btnPublish_Click(object sender, EventArgs e)
        {
            if (_currentRecipe.RecipeID == 0)
            {
                MessageBox.Show("Előbb mentsd el tervezetként a receptet.");
                return;
            }

            var confirm = MessageBox.Show(
                "Biztosan publikálod a receptet? Megjelenik a weboldalon.",
                "Publikálás megerősítése",
                MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _currentRecipe = await _recipeService.PublishRecipeAsync(_currentRecipe.RecipeID);

                UpdateStatusPills(_currentRecipe.Status);
                MessageBox.Show("Recept sikeresen publikálva.");

                // Bundle visszajelzés – EGYELŐRE KOMMENTÁLVA
                // Ha a bundle végpont elkészül, itt jelenik meg a visszajelzés:
                //
                // if (_currentRecipe.BundleBvin.HasValue)
                //     MessageBox.Show($"Bundle létrehozva: {_currentRecipe.BundleBvin}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Publikálási hiba: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Visszavonás
        // ------------------------------------------------------------------

        private async void btnRevoke_Click(object sender, EventArgs e)
        {
            if (_currentRecipe.RecipeID == 0) return;

            var confirm = MessageBox.Show(
                "Biztosan visszavonod a receptet? Eltűnik a weboldalról.",
                "Visszavonás megerősítése",
                MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes) return;

            try
            {
                await _recipeService.RevokeRecipeAsync(_currentRecipe.RecipeID);
                _currentRecipe.Status = "Revoked";
                UpdateStatusPills("Revoked");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Visszavonási hiba: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------
        // Segéd metódusok
        // ------------------------------------------------------------------

        private void NewRecipe()
        {
            _currentRecipe = new Recipe
            {
                Status   = "Draft",
                Servings = 4
            };
            RefreshIngredientGrid();
            UpdateStatusPills("Draft");
        }

        private void ReadFormToRecipe()
        {
            _currentRecipe.RecipeName      = txtRecipeName.Text.Trim();
            _currentRecipe.Description     = txtDescription.Text.Trim();
            _currentRecipe.Category        = txtCategory.Text.Trim();
            _currentRecipe.AuthorName      = txtAuthor.Text.Trim();
            _currentRecipe.Tags            = txtTags.Text.Trim();
            _currentRecipe.Servings        = (int)nudServings.Value;
            _currentRecipe.PrepTimeMinutes = (int)nudPrepTime.Value;
            _currentRecipe.CookTimeMinutes = (int)nudCookTime.Value;
            _currentRecipe.TotalCalories   = nudTotalCalories.Value > 0 ? (int)nudTotalCalories.Value : (int?)null;
            _currentRecipe.Steps           = txtSteps.Text.Trim();
        }

        // ------------------------------------------------------------------
        // Státusz pill kiemelés
        // ------------------------------------------------------------------

        private void UpdateStatusPills(string status)
        {
            var activeColor   = System.Drawing.Color.FromArgb(107, 142, 35);
            var inactiveColor = System.Drawing.Color.FromArgb(136, 136, 136);

            lblStatusPillPublished.ForeColor = status == "Published" ? activeColor : inactiveColor;
            lblStatusPillDraft.ForeColor     = status == "Draft"     ? activeColor : inactiveColor;
            lblStatusPillRevoked.ForeColor   = status == "Revoked"   ? System.Drawing.Color.FromArgb(180, 70, 70) : inactiveColor;

            lblStatus.Text = status switch
            {
                "Published" => "Állapot: Publikált",
                "Revoked"   => "Állapot: Visszavont",
                _           => "Állapot: Tervezet"
            };
        }

        private void RefreshIngredientGrid()
        {
            dgvIngredients.DataSource = null;
            dgvIngredients.DataSource = _currentRecipe.Ingredients
                .OrderBy(i => i.SortOrder)
                .ToList();
        }
    }
}
