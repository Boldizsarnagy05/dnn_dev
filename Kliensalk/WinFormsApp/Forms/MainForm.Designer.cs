namespace NaturaCo.RecipeEditor.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label           lblRecipeNameCaption;
        private System.Windows.Forms.TextBox         txtRecipeName;
        private System.Windows.Forms.Label           lblStatusCaption;
        private System.Windows.Forms.ComboBox        cmbStatus;

        private System.Windows.Forms.Label           lblCategory;
        private System.Windows.Forms.ComboBox        cmbMealType;

        private System.Windows.Forms.Label           lblPrepTime;
        private System.Windows.Forms.NumericUpDown   nudPrepTime;
        private System.Windows.Forms.Label           lblCookTime;
        private System.Windows.Forms.NumericUpDown   nudCookTime;

        private System.Windows.Forms.Label           lblServings;
        private System.Windows.Forms.NumericUpDown   nudServings;

        private System.Windows.Forms.Label           lblDescription;
        private System.Windows.Forms.TextBox         txtDescription;

        private System.Windows.Forms.Label           lblCatalog;
        private System.Windows.Forms.Label           lblCategoryFilter;
        private System.Windows.Forms.ComboBox        cmbCategory;
        private System.Windows.Forms.Label           lblProductsLabel;
        private System.Windows.Forms.ListBox         lstProducts;
        private System.Windows.Forms.Label           lblCatalogHint;

        private System.Windows.Forms.Label           lblIngredients;
        private System.Windows.Forms.Button          btnAddCustomIngredient;
        private System.Windows.Forms.DataGridView    dgvIngredients;

        private System.Windows.Forms.Label           lblSteps;
        private System.Windows.Forms.TextBox         txtSteps;

        private System.Windows.Forms.Label           lblTotalCost;
        private System.Windows.Forms.Label           lblCostPerServing;
        private System.Windows.Forms.Label           lblTotalCalories;
        private System.Windows.Forms.Label           lblCaloriesPerServing;
        private System.Windows.Forms.Button          btnSaveDraft;
        private System.Windows.Forms.Button          btnPublish;
        private System.Windows.Forms.Button          btnRevoke;

        private System.Windows.Forms.Label           lblStatus;

        private System.Windows.Forms.GroupBox        grpRecipes;
        private System.Windows.Forms.ListView        lvRecipes;
        private System.Windows.Forms.Button          btnNewRecipe;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblRecipeNameCaption = new System.Windows.Forms.Label();
            this.txtRecipeName = new System.Windows.Forms.TextBox();
            this.lblStatusCaption = new System.Windows.Forms.Label();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbMealType = new System.Windows.Forms.ComboBox();
            this.lblPrepTime = new System.Windows.Forms.Label();
            this.nudPrepTime = new System.Windows.Forms.NumericUpDown();
            this.lblCookTime = new System.Windows.Forms.Label();
            this.nudCookTime = new System.Windows.Forms.NumericUpDown();
            this.lblServings = new System.Windows.Forms.Label();
            this.nudServings = new System.Windows.Forms.NumericUpDown();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblCatalog = new System.Windows.Forms.Label();
            this.lblCategoryFilter = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblProductsLabel = new System.Windows.Forms.Label();
            this.lstProducts = new System.Windows.Forms.ListBox();
            this.lblCatalogHint = new System.Windows.Forms.Label();
            this.lblIngredients = new System.Windows.Forms.Label();
            this.btnAddCustomIngredient = new System.Windows.Forms.Button();
            this.dgvIngredients = new System.Windows.Forms.DataGridView();
            this.lblSteps = new System.Windows.Forms.Label();
            this.txtSteps = new System.Windows.Forms.TextBox();
            this.lblTotalCost = new System.Windows.Forms.Label();
            this.lblCostPerServing = new System.Windows.Forms.Label();
            this.lblTotalCalories = new System.Windows.Forms.Label();
            this.lblCaloriesPerServing = new System.Windows.Forms.Label();
            this.btnSaveDraft = new System.Windows.Forms.Button();
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnRevoke = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grpRecipes = new System.Windows.Forms.GroupBox();
            this.lvRecipes = new System.Windows.Forms.ListView();
            this.btnNewRecipe = new System.Windows.Forms.Button();
            this.grpRecipes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrepTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCookTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudServings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIngredients)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRecipeNameCaption
            // 
            this.lblRecipeNameCaption.AutoSize = true;
            this.lblRecipeNameCaption.Location = new System.Drawing.Point(20, 15);
            this.lblRecipeNameCaption.Name = "lblRecipeNameCaption";
            this.lblRecipeNameCaption.Size = new System.Drawing.Size(150, 32);
            this.lblRecipeNameCaption.TabIndex = 0;
            this.lblRecipeNameCaption.Text = "Recept neve:";
            // 
            // txtRecipeName
            // 
            this.txtRecipeName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.txtRecipeName.Location = new System.Drawing.Point(20, 35);
            this.txtRecipeName.Name = "txtRecipeName";
            this.txtRecipeName.Size = new System.Drawing.Size(455, 47);
            this.txtRecipeName.TabIndex = 1;
            // 
            // lblStatusCaption
            // 
            this.lblStatusCaption.Location = new System.Drawing.Point(0, 0);
            this.lblStatusCaption.Name = "lblStatusCaption";
            this.lblStatusCaption.Size = new System.Drawing.Size(100, 23);
            this.lblStatusCaption.TabIndex = 2;
            this.lblStatusCaption.Visible = false;
            // 
            // cmbStatus
            // 
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.Items.AddRange(new object[] {
            "Draft",
            "Published",
            "Revoked"});
            this.cmbStatus.Location = new System.Drawing.Point(0, 0);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(121, 40);
            this.cmbStatus.TabIndex = 3;
            this.cmbStatus.Visible = false;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(481, 15);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(120, 32);
            this.lblCategory.TabIndex = 4;
            this.lblCategory.Text = "Kategória:";
            //
            // cmbMealType
            //
            this.cmbMealType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMealType.Items.AddRange(new object[] {
            "Reggeli",
            "Ebéd",
            "Vacsora",
            "Nassolnivaló"});
            this.cmbMealType.Location = new System.Drawing.Point(487, 43);
            this.cmbMealType.Name = "cmbMealType";
            this.cmbMealType.Size = new System.Drawing.Size(505, 40);
            this.cmbMealType.TabIndex = 5;
            // 
            // lblPrepTime
            // 
            this.lblPrepTime.AutoSize = true;
            this.lblPrepTime.Location = new System.Drawing.Point(20, 95);
            this.lblPrepTime.Name = "lblPrepTime";
            this.lblPrepTime.Size = new System.Drawing.Size(187, 32);
            this.lblPrepTime.TabIndex = 8;
            this.lblPrepTime.Text = "Elkészítés (perc):";
            // 
            // nudPrepTime
            // 
            this.nudPrepTime.Location = new System.Drawing.Point(20, 128);
            this.nudPrepTime.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudPrepTime.Name = "nudPrepTime";
            this.nudPrepTime.Size = new System.Drawing.Size(267, 39);
            this.nudPrepTime.TabIndex = 9;
            // 
            // lblCookTime
            // 
            this.lblCookTime.AutoSize = true;
            this.lblCookTime.Location = new System.Drawing.Point(324, 95);
            this.lblCookTime.Name = "lblCookTime";
            this.lblCookTime.Size = new System.Drawing.Size(209, 32);
            this.lblCookTime.TabIndex = 10;
            this.lblCookTime.Text = "Sütés/főzés (perc):";
            // 
            // nudCookTime
            // 
            this.nudCookTime.Location = new System.Drawing.Point(330, 130);
            this.nudCookTime.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudCookTime.Name = "nudCookTime";
            this.nudCookTime.Size = new System.Drawing.Size(271, 39);
            this.nudCookTime.TabIndex = 11;
            // 
            // lblServings
            // 
            this.lblServings.AutoSize = true;
            this.lblServings.Location = new System.Drawing.Point(651, 95);
            this.lblServings.Name = "lblServings";
            this.lblServings.Size = new System.Drawing.Size(74, 32);
            this.lblServings.TabIndex = 12;
            this.lblServings.Text = "Adag:";
            // 
            // nudServings
            // 
            this.nudServings.Location = new System.Drawing.Point(657, 128);
            this.nudServings.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudServings.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudServings.Name = "nudServings";
            this.nudServings.Size = new System.Drawing.Size(267, 39);
            this.nudServings.TabIndex = 13;
            this.nudServings.ValueChanged += new System.EventHandler(this.nudServings_ValueChanged);
            this.nudServings.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(20, 193);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(79, 32);
            this.lblDescription.TabIndex = 16;
            this.lblDescription.Text = "Leírás:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(20, 215);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(940, 49);
            this.txtDescription.TabIndex = 17;
            // 
            // lblCatalog
            // 
            this.lblCatalog.AutoSize = true;
            this.lblCatalog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCatalog.Location = new System.Drawing.Point(20, 296);
            this.lblCatalog.Name = "lblCatalog";
            this.lblCatalog.Size = new System.Drawing.Size(208, 32);
            this.lblCatalog.TabIndex = 20;
            this.lblCatalog.Text = "Termékkatalógus";
            // 
            // lblCategoryFilter
            // 
            this.lblCategoryFilter.AutoSize = true;
            this.lblCategoryFilter.Location = new System.Drawing.Point(20, 328);
            this.lblCategoryFilter.Name = "lblCategoryFilter";
            this.lblCategoryFilter.Size = new System.Drawing.Size(120, 32);
            this.lblCategoryFilter.TabIndex = 21;
            this.lblCategoryFilter.Text = "Kategória:";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Location = new System.Drawing.Point(20, 340);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(290, 40);
            this.cmbCategory.TabIndex = 22;
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);
            // 
            // lblProductsLabel
            // 
            this.lblProductsLabel.AutoSize = true;
            this.lblProductsLabel.Location = new System.Drawing.Point(20, 383);
            this.lblProductsLabel.Name = "lblProductsLabel";
            this.lblProductsLabel.Size = new System.Drawing.Size(122, 32);
            this.lblProductsLabel.TabIndex = 23;
            this.lblProductsLabel.Text = "Termékek:";
            // 
            // lstProducts
            // 
            this.lstProducts.FormattingEnabled = true;
            this.lstProducts.ItemHeight = 32;
            this.lstProducts.Location = new System.Drawing.Point(20, 403);
            this.lstProducts.Name = "lstProducts";
            this.lstProducts.Size = new System.Drawing.Size(290, 100);
            this.lstProducts.TabIndex = 24;
            this.lstProducts.DoubleClick += new System.EventHandler(this.lstProducts_DoubleClick);
            // 
            // lblCatalogHint
            // 
            this.lblCatalogHint.AutoSize = true;
            this.lblCatalogHint.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            this.lblCatalogHint.Location = new System.Drawing.Point(21, 541);
            this.lblCatalogHint.Name = "lblCatalogHint";
            this.lblCatalogHint.Size = new System.Drawing.Size(408, 30);
            this.lblCatalogHint.TabIndex = 25;
            this.lblCatalogHint.Text = "Dupla kattintás → hozzáadás a recepthez";
            //
            // lblIngredients
            //
            this.lblIngredients.AutoSize = true;
            this.lblIngredients.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblIngredients.Location = new System.Drawing.Point(330, 303);
            this.lblIngredients.Name = "lblIngredients";
            this.lblIngredients.Size = new System.Drawing.Size(145, 32);
            this.lblIngredients.TabIndex = 26;
            this.lblIngredients.Text = "Hozzávalók";
            //
            // btnAddCustomIngredient
            //
            this.btnAddCustomIngredient.Location = new System.Drawing.Point(20, 575);
            this.btnAddCustomIngredient.Name = "btnAddCustomIngredient";
            this.btnAddCustomIngredient.Size = new System.Drawing.Size(290, 34);
            this.btnAddCustomIngredient.TabIndex = 50;
            this.btnAddCustomIngredient.Text = "+ Egyedi hozzávaló";
            this.btnAddCustomIngredient.Click += new System.EventHandler(this.btnAddCustomIngredient_Click);
            //
            // dgvIngredients
            // 
            this.dgvIngredients.AllowUserToAddRows = false;
            this.dgvIngredients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIngredients.Location = new System.Drawing.Point(330, 328);
            this.dgvIngredients.Name = "dgvIngredients";
            this.dgvIngredients.RowHeadersVisible = false;
            this.dgvIngredients.RowHeadersWidth = 82;
            this.dgvIngredients.Size = new System.Drawing.Size(630, 200);
            this.dgvIngredients.TabIndex = 27;
            // 
            // lblSteps
            // 
            this.lblSteps.AutoSize = true;
            this.lblSteps.Location = new System.Drawing.Point(20, 583);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Size = new System.Drawing.Size(202, 32);
            this.lblSteps.TabIndex = 28;
            this.lblSteps.Text = "Elkészítés lépései:";
            // 
            // txtSteps
            // 
            this.txtSteps.Location = new System.Drawing.Point(20, 603);
            this.txtSteps.Multiline = true;
            this.txtSteps.Name = "txtSteps";
            this.txtSteps.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSteps.Size = new System.Drawing.Size(940, 115);
            this.txtSteps.TabIndex = 29;
            // 
            // lblTotalCost
            // 
            this.lblTotalCost.AutoSize = true;
            this.lblTotalCost.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalCost.Location = new System.Drawing.Point(20, 721);
            this.lblTotalCost.Name = "lblTotalCost";
            this.lblTotalCost.Size = new System.Drawing.Size(242, 32);
            this.lblTotalCost.TabIndex = 30;
            this.lblTotalCost.Text = "Becsült költség: 0 Ft";
            // 
            // lblCostPerServing
            // 
            this.lblCostPerServing.AutoSize = true;
            this.lblCostPerServing.Location = new System.Drawing.Point(310, 721);
            this.lblCostPerServing.Name = "lblCostPerServing";
            this.lblCostPerServing.Size = new System.Drawing.Size(196, 32);
            this.lblCostPerServing.TabIndex = 31;
            this.lblCostPerServing.Text = "Adagonként: 0 Ft";
            // 
            // lblTotalCalories
            // 
            this.lblTotalCalories.AutoSize = true;
            this.lblTotalCalories.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalCalories.Location = new System.Drawing.Point(20, 762);
            this.lblTotalCalories.Name = "lblTotalCalories";
            this.lblTotalCalories.Size = new System.Drawing.Size(238, 32);
            this.lblTotalCalories.TabIndex = 36;
            this.lblTotalCalories.Text = "Össz. kalória: 0 kcal";
            // 
            // lblCaloriesPerServing
            // 
            this.lblCaloriesPerServing.AutoSize = true;
            this.lblCaloriesPerServing.Location = new System.Drawing.Point(310, 762);
            this.lblCaloriesPerServing.Name = "lblCaloriesPerServing";
            this.lblCaloriesPerServing.Size = new System.Drawing.Size(217, 32);
            this.lblCaloriesPerServing.TabIndex = 37;
            this.lblCaloriesPerServing.Text = "Adagonként: 0 kcal";
            // 
            // btnSaveDraft
            // 
            this.btnSaveDraft.Location = new System.Drawing.Point(557, 721);
            this.btnSaveDraft.Name = "btnSaveDraft";
            this.btnSaveDraft.Size = new System.Drawing.Size(150, 35);
            this.btnSaveDraft.TabIndex = 32;
            this.btnSaveDraft.Text = "Mentés tervezetként";
            this.btnSaveDraft.UseVisualStyleBackColor = true;
            this.btnSaveDraft.Click += new System.EventHandler(this.btnSaveDraft_Click);
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(715, 721);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(120, 35);
            this.btnPublish.TabIndex = 33;
            this.btnPublish.Text = "Publikálás";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // btnRevoke
            // 
            this.btnRevoke.Location = new System.Drawing.Point(843, 721);
            this.btnRevoke.Name = "btnRevoke";
            this.btnRevoke.Size = new System.Drawing.Size(130, 35);
            this.btnRevoke.TabIndex = 34;
            this.btnRevoke.Text = "Visszavonás";
            this.btnRevoke.UseVisualStyleBackColor = true;
            this.btnRevoke.Click += new System.EventHandler(this.btnRevoke_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 23);
            this.lblStatus.TabIndex = 35;
            this.lblStatus.Visible = false;
            //
            // grpRecipes
            //
            this.grpRecipes.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Right |
                System.Windows.Forms.AnchorStyles.Bottom));
            this.grpRecipes.Controls.Add(this.lvRecipes);
            this.grpRecipes.Controls.Add(this.btnNewRecipe);
            this.grpRecipes.Location = new System.Drawing.Point(1010, 10);
            this.grpRecipes.Name = "grpRecipes";
            this.grpRecipes.Size = new System.Drawing.Size(290, 790);
            this.grpRecipes.TabIndex = 50;
            this.grpRecipes.TabStop = false;
            this.grpRecipes.Text = "Meglévő receptek";
            //
            // lvRecipes
            //
            this.lvRecipes.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right |
                System.Windows.Forms.AnchorStyles.Bottom));
            this.lvRecipes.FullRowSelect = true;
            this.lvRecipes.GridLines = true;
            this.lvRecipes.HideSelection = false;
            this.lvRecipes.Location = new System.Drawing.Point(8, 22);
            this.lvRecipes.MultiSelect = false;
            this.lvRecipes.Name = "lvRecipes";
            this.lvRecipes.Size = new System.Drawing.Size(272, 728);
            this.lvRecipes.TabIndex = 51;
            this.lvRecipes.UseCompatibleStateImageBehavior = false;
            this.lvRecipes.View = System.Windows.Forms.View.Details;
            this.lvRecipes.SelectedIndexChanged += new System.EventHandler(this.lvRecipes_SelectedIndexChanged);
            //
            // btnNewRecipe
            //
            this.btnNewRecipe.Anchor = ((System.Windows.Forms.AnchorStyles)(
                System.Windows.Forms.AnchorStyles.Bottom |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right));
            this.btnNewRecipe.Location = new System.Drawing.Point(8, 756);
            this.btnNewRecipe.Name = "btnNewRecipe";
            this.btnNewRecipe.Size = new System.Drawing.Size(272, 28);
            this.btnNewRecipe.TabIndex = 52;
            this.btnNewRecipe.Text = "Új recept";
            this.btnNewRecipe.UseVisualStyleBackColor = true;
            this.btnNewRecipe.Click += new System.EventHandler(this.btnNewRecipe_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 805);
            this.Controls.Add(this.lblRecipeNameCaption);
            this.Controls.Add(this.txtRecipeName);
            this.Controls.Add(this.lblStatusCaption);
            this.Controls.Add(this.cmbStatus);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.cmbMealType);
            this.Controls.Add(this.lblPrepTime);
            this.Controls.Add(this.nudPrepTime);
            this.Controls.Add(this.lblCookTime);
            this.Controls.Add(this.nudCookTime);
            this.Controls.Add(this.lblServings);
            this.Controls.Add(this.nudServings);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblCatalog);
            this.Controls.Add(this.lblCategoryFilter);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lblProductsLabel);
            this.Controls.Add(this.lstProducts);
            this.Controls.Add(this.lblCatalogHint);
            this.Controls.Add(this.lblIngredients);
            this.Controls.Add(this.btnAddCustomIngredient);
            this.Controls.Add(this.dgvIngredients);
            this.Controls.Add(this.lblSteps);
            this.Controls.Add(this.txtSteps);
            this.Controls.Add(this.lblTotalCost);
            this.Controls.Add(this.lblCostPerServing);
            this.Controls.Add(this.lblTotalCalories);
            this.Controls.Add(this.lblCaloriesPerServing);
            this.Controls.Add(this.btnSaveDraft);
            this.Controls.Add(this.btnPublish);
            this.Controls.Add(this.btnRevoke);
            this.Controls.Add(this.grpRecipes);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NaturaCo – Recept Szerkesztő";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudPrepTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCookTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudServings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIngredients)).EndInit();
            this.grpRecipes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
