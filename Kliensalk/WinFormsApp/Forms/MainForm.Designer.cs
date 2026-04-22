namespace NaturaCo.RecipeEditor.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Fejléc
        private System.Windows.Forms.Panel         pnlHeader;
        private System.Windows.Forms.PictureBox    picPreview;
        private System.Windows.Forms.TextBox       txtRecipeName;
        private System.Windows.Forms.Label         lblStatusPillPublished;
        private System.Windows.Forms.Label         lblStatusPillDraft;
        private System.Windows.Forms.Label         lblStatusPillRevoked;
        private System.Windows.Forms.Button        btnSaveDraft;
        private System.Windows.Forms.Button        btnPublish;
        private System.Windows.Forms.Button        btnRevoke;

        // Fő elrendezés
        private System.Windows.Forms.SplitContainer splitMain;

        // Bal oldal – termékkatalógus
        private System.Windows.Forms.Panel         pnlCatalog;
        private System.Windows.Forms.Label         lblCatalogTitle;
        private System.Windows.Forms.Label         lblCategoryCaption;
        private System.Windows.Forms.ComboBox      cmbCategory;
        private System.Windows.Forms.Label         lblProductsCaption;
        private System.Windows.Forms.ListBox       lstProducts;
        private System.Windows.Forms.Label         lblCatalogHint;

        // Jobb oldal – recept szerkesztő (görgethető)
        private System.Windows.Forms.Panel         pnlEditor;

        // Metaadatok kártya
        private System.Windows.Forms.Panel         pnlMetadata;
        private System.Windows.Forms.Label         lblMetadataTitle;
        private System.Windows.Forms.Label         lblCategoryField;
        private System.Windows.Forms.TextBox       txtCategory;
        private System.Windows.Forms.Label         lblAuthorField;
        private System.Windows.Forms.TextBox       txtAuthor;
        private System.Windows.Forms.Label         lblPrepField;
        private System.Windows.Forms.NumericUpDown nudPrepTime;
        private System.Windows.Forms.Label         lblCookField;
        private System.Windows.Forms.NumericUpDown nudCookTime;
        private System.Windows.Forms.Label         lblServingsField;
        private System.Windows.Forms.NumericUpDown nudServings;
        private System.Windows.Forms.Label         lblCaloriesField;
        private System.Windows.Forms.NumericUpDown nudTotalCalories;
        private System.Windows.Forms.Label         lblDescriptionField;
        private System.Windows.Forms.TextBox       txtDescription;
        private System.Windows.Forms.Label         lblPreviewUrlField;
        private System.Windows.Forms.TextBox       txtPreviewImageUrl;

        // Tag-ek kártya
        private System.Windows.Forms.Panel         pnlTags;
        private System.Windows.Forms.Label         lblTagsTitle;
        private System.Windows.Forms.TextBox       txtTags;
        private System.Windows.Forms.Label         lblTagsHint;

        // Összetevők kártya
        private System.Windows.Forms.Panel         pnlIngredients;
        private System.Windows.Forms.Label         lblIngredientsTitle;
        private System.Windows.Forms.Label         lblIngredientsHint;
        private System.Windows.Forms.DataGridView  dgvIngredients;

        // Elkészítés kártya
        private System.Windows.Forms.Panel         pnlSteps;
        private System.Windows.Forms.Label         lblStepsTitle;
        private System.Windows.Forms.TextBox       txtSteps;

        // Összegzés kártya
        private System.Windows.Forms.Panel         pnlTotals;
        private System.Windows.Forms.Label         lblTotalCost;
        private System.Windows.Forms.Label         lblCostPerServing;

        // Lábléc
        private System.Windows.Forms.Panel         pnlFooter;
        private System.Windows.Forms.Label         lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ---------- Kontrollok példányosítása ----------
            this.pnlHeader              = new System.Windows.Forms.Panel();
            this.picPreview             = new System.Windows.Forms.PictureBox();
            this.txtRecipeName          = new System.Windows.Forms.TextBox();
            this.lblStatusPillPublished = new System.Windows.Forms.Label();
            this.lblStatusPillDraft     = new System.Windows.Forms.Label();
            this.lblStatusPillRevoked   = new System.Windows.Forms.Label();
            this.btnSaveDraft           = new System.Windows.Forms.Button();
            this.btnPublish             = new System.Windows.Forms.Button();
            this.btnRevoke              = new System.Windows.Forms.Button();

            this.splitMain              = new System.Windows.Forms.SplitContainer();

            this.pnlCatalog             = new System.Windows.Forms.Panel();
            this.lblCatalogTitle        = new System.Windows.Forms.Label();
            this.lblCategoryCaption     = new System.Windows.Forms.Label();
            this.cmbCategory            = new System.Windows.Forms.ComboBox();
            this.lblProductsCaption     = new System.Windows.Forms.Label();
            this.lstProducts            = new System.Windows.Forms.ListBox();
            this.lblCatalogHint         = new System.Windows.Forms.Label();

            this.pnlEditor              = new System.Windows.Forms.Panel();

            this.pnlMetadata            = new System.Windows.Forms.Panel();
            this.lblMetadataTitle       = new System.Windows.Forms.Label();
            this.lblCategoryField       = new System.Windows.Forms.Label();
            this.txtCategory            = new System.Windows.Forms.TextBox();
            this.lblAuthorField         = new System.Windows.Forms.Label();
            this.txtAuthor              = new System.Windows.Forms.TextBox();
            this.lblPrepField           = new System.Windows.Forms.Label();
            this.nudPrepTime            = new System.Windows.Forms.NumericUpDown();
            this.lblCookField           = new System.Windows.Forms.Label();
            this.nudCookTime            = new System.Windows.Forms.NumericUpDown();
            this.lblServingsField       = new System.Windows.Forms.Label();
            this.nudServings            = new System.Windows.Forms.NumericUpDown();
            this.lblCaloriesField       = new System.Windows.Forms.Label();
            this.nudTotalCalories       = new System.Windows.Forms.NumericUpDown();
            this.lblDescriptionField    = new System.Windows.Forms.Label();
            this.txtDescription         = new System.Windows.Forms.TextBox();
            this.lblPreviewUrlField     = new System.Windows.Forms.Label();
            this.txtPreviewImageUrl     = new System.Windows.Forms.TextBox();

            this.pnlTags                = new System.Windows.Forms.Panel();
            this.lblTagsTitle           = new System.Windows.Forms.Label();
            this.txtTags                = new System.Windows.Forms.TextBox();
            this.lblTagsHint            = new System.Windows.Forms.Label();

            this.pnlIngredients         = new System.Windows.Forms.Panel();
            this.lblIngredientsTitle    = new System.Windows.Forms.Label();
            this.lblIngredientsHint     = new System.Windows.Forms.Label();
            this.dgvIngredients         = new System.Windows.Forms.DataGridView();

            this.pnlSteps               = new System.Windows.Forms.Panel();
            this.lblStepsTitle          = new System.Windows.Forms.Label();
            this.txtSteps               = new System.Windows.Forms.TextBox();

            this.pnlTotals              = new System.Windows.Forms.Panel();
            this.lblTotalCost           = new System.Windows.Forms.Label();
            this.lblCostPerServing      = new System.Windows.Forms.Label();

            this.pnlFooter              = new System.Windows.Forms.Panel();
            this.lblStatus              = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlCatalog.SuspendLayout();
            this.pnlEditor.SuspendLayout();
            this.pnlMetadata.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrepTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCookTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudServings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalCalories)).BeginInit();
            this.pnlTags.SuspendLayout();
            this.pnlIngredients.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIngredients)).BeginInit();
            this.pnlSteps.SuspendLayout();
            this.pnlTotals.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ===================================================================
            // FEJLÉC
            // ===================================================================

            // picPreview
            this.picPreview.BackColor   = System.Drawing.Color.FromArgb(229, 229, 224);
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location    = new System.Drawing.Point(20, 15);
            this.picPreview.Size        = new System.Drawing.Size(70, 70);
            this.picPreview.SizeMode    = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // txtRecipeName
            this.txtRecipeName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtRecipeName.Font        = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.txtRecipeName.ForeColor   = System.Drawing.Color.FromArgb(44, 44, 44);
            this.txtRecipeName.BackColor   = System.Drawing.Color.White;
            this.txtRecipeName.Location    = new System.Drawing.Point(110, 22);
            this.txtRecipeName.Size        = new System.Drawing.Size(520, 32);

            // Status pillek – egyszerű címkék, a lblStatus a jelenlegi állapot szövege
            ConfigurePill(this.lblStatusPillPublished, "Published", 110, 60, System.Drawing.Color.FromArgb(107, 142, 35));
            ConfigurePill(this.lblStatusPillDraft,     "Draft",     205, 60, System.Drawing.Color.FromArgb(136, 136, 136));
            ConfigurePill(this.lblStatusPillRevoked,   "Revoked",   275, 60, System.Drawing.Color.FromArgb(136, 136, 136));

            // btnSaveDraft
            ConfigurePrimaryButton(this.btnSaveDraft, "Mentés tervezetként", System.Drawing.Color.FromArgb(139, 111, 60));
            this.btnSaveDraft.Location = new System.Drawing.Point(750, 28);
            this.btnSaveDraft.Size     = new System.Drawing.Size(160, 40);
            this.btnSaveDraft.Click   += new System.EventHandler(this.btnSaveDraft_Click);

            // btnPublish
            ConfigurePrimaryButton(this.btnPublish, "Publikálás", System.Drawing.Color.FromArgb(107, 142, 35));
            this.btnPublish.Location = new System.Drawing.Point(920, 28);
            this.btnPublish.Size     = new System.Drawing.Size(120, 40);
            this.btnPublish.Click   += new System.EventHandler(this.btnPublish_Click);

            // btnRevoke
            ConfigurePrimaryButton(this.btnRevoke, "Visszavonás", System.Drawing.Color.FromArgb(180, 70, 70));
            this.btnRevoke.Location = new System.Drawing.Point(1050, 28);
            this.btnRevoke.Size     = new System.Drawing.Size(130, 40);
            this.btnRevoke.Click   += new System.EventHandler(this.btnRevoke_Click);

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 100;
            this.pnlHeader.Controls.Add(this.picPreview);
            this.pnlHeader.Controls.Add(this.txtRecipeName);
            this.pnlHeader.Controls.Add(this.lblStatusPillPublished);
            this.pnlHeader.Controls.Add(this.lblStatusPillDraft);
            this.pnlHeader.Controls.Add(this.lblStatusPillRevoked);
            this.pnlHeader.Controls.Add(this.btnSaveDraft);
            this.pnlHeader.Controls.Add(this.btnPublish);
            this.pnlHeader.Controls.Add(this.btnRevoke);

            // ===================================================================
            // BAL OLDAL – TERMÉKKATALÓGUS
            // ===================================================================

            // lblCatalogTitle
            this.lblCatalogTitle.AutoSize  = true;
            this.lblCatalogTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblCatalogTitle.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblCatalogTitle.Location  = new System.Drawing.Point(20, 20);
            this.lblCatalogTitle.Text      = "TERMÉKKATALÓGUS";

            // lblCategoryCaption
            this.lblCategoryCaption.AutoSize  = true;
            this.lblCategoryCaption.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblCategoryCaption.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblCategoryCaption.Location  = new System.Drawing.Point(20, 55);
            this.lblCategoryCaption.Text      = "KATEGÓRIA";

            // cmbCategory
            this.cmbCategory.DropDownStyle         = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FlatStyle             = System.Windows.Forms.FlatStyle.Flat;
            this.cmbCategory.Font                  = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCategory.Location              = new System.Drawing.Point(20, 78);
            this.cmbCategory.Size                  = new System.Drawing.Size(300, 28);
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);

            // lblProductsCaption
            this.lblProductsCaption.AutoSize  = true;
            this.lblProductsCaption.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblProductsCaption.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblProductsCaption.Location  = new System.Drawing.Point(20, 125);
            this.lblProductsCaption.Text      = "TERMÉKEK";

            // lstProducts
            this.lstProducts.BorderStyle  = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstProducts.Font         = new System.Drawing.Font("Segoe UI", 10F);
            this.lstProducts.ItemHeight   = 22;
            this.lstProducts.Location     = new System.Drawing.Point(20, 148);
            this.lstProducts.Size         = new System.Drawing.Size(300, 400);
            this.lstProducts.Anchor       = System.Windows.Forms.AnchorStyles.Top
                                          | System.Windows.Forms.AnchorStyles.Bottom
                                          | System.Windows.Forms.AnchorStyles.Left
                                          | System.Windows.Forms.AnchorStyles.Right;
            this.lstProducts.DoubleClick += new System.EventHandler(this.lstProducts_DoubleClick);

            // lblCatalogHint
            this.lblCatalogHint.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            this.lblCatalogHint.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblCatalogHint.Location  = new System.Drawing.Point(20, 555);
            this.lblCatalogHint.Size      = new System.Drawing.Size(300, 20);
            this.lblCatalogHint.Anchor    = System.Windows.Forms.AnchorStyles.Bottom
                                          | System.Windows.Forms.AnchorStyles.Left
                                          | System.Windows.Forms.AnchorStyles.Right;
            this.lblCatalogHint.Text      = "Dupla kattintás a recepthez adáshoz →";

            // pnlCatalog
            this.pnlCatalog.BackColor = System.Drawing.Color.White;
            this.pnlCatalog.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlCatalog.Padding   = new System.Windows.Forms.Padding(10);
            this.pnlCatalog.Controls.Add(this.lblCatalogTitle);
            this.pnlCatalog.Controls.Add(this.lblCategoryCaption);
            this.pnlCatalog.Controls.Add(this.cmbCategory);
            this.pnlCatalog.Controls.Add(this.lblProductsCaption);
            this.pnlCatalog.Controls.Add(this.lstProducts);
            this.pnlCatalog.Controls.Add(this.lblCatalogHint);

            // ===================================================================
            // JOBB OLDAL – SZERKESZTŐ (görgethető)
            // ===================================================================

            // pnlMetadata
            ConfigureCard(this.pnlMetadata, 10, 10, 700, 340);

            // lblMetadataTitle
            this.lblMetadataTitle.AutoSize  = true;
            this.lblMetadataTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblMetadataTitle.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblMetadataTitle.Location  = new System.Drawing.Point(20, 15);
            this.lblMetadataTitle.Text      = "RECEPT METAADATOK";

            // Mezők – két oszlopos elrendezés
            ConfigureFieldLabel(this.lblCategoryField, "KATEGÓRIA", 20, 50);
            ConfigureFieldTextBox(this.txtCategory, 20, 72, 315);

            ConfigureFieldLabel(this.lblAuthorField, "SZERZŐ", 355, 50);
            ConfigureFieldTextBox(this.txtAuthor, 355, 72, 315);

            ConfigureFieldLabel(this.lblPrepField, "ELŐKÉSZÍTÉSI IDŐ (PERC)", 20, 115);
            ConfigureFieldNumeric(this.nudPrepTime, 20, 137, 315, 0, 600);

            ConfigureFieldLabel(this.lblCookField, "FŐZÉSI IDŐ (PERC)", 355, 115);
            ConfigureFieldNumeric(this.nudCookTime, 355, 137, 315, 0, 600);

            ConfigureFieldLabel(this.lblServingsField, "ALAP ADAG", 20, 180);
            ConfigureFieldNumeric(this.nudServings, 20, 202, 315, 1, 50);
            this.nudServings.Value = 4;

            ConfigureFieldLabel(this.lblCaloriesField, "ÖSSZ KALÓRIA", 355, 180);
            ConfigureFieldNumeric(this.nudTotalCalories, 355, 202, 315, 0, 10000);

            ConfigureFieldLabel(this.lblDescriptionField, "LEÍRÁS", 20, 245);
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.Location    = new System.Drawing.Point(20, 267);
            this.txtDescription.Multiline   = true;
            this.txtDescription.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size        = new System.Drawing.Size(650, 60);

            this.pnlMetadata.Controls.Add(this.lblMetadataTitle);
            this.pnlMetadata.Controls.Add(this.lblCategoryField);
            this.pnlMetadata.Controls.Add(this.txtCategory);
            this.pnlMetadata.Controls.Add(this.lblAuthorField);
            this.pnlMetadata.Controls.Add(this.txtAuthor);
            this.pnlMetadata.Controls.Add(this.lblPrepField);
            this.pnlMetadata.Controls.Add(this.nudPrepTime);
            this.pnlMetadata.Controls.Add(this.lblCookField);
            this.pnlMetadata.Controls.Add(this.nudCookTime);
            this.pnlMetadata.Controls.Add(this.lblServingsField);
            this.pnlMetadata.Controls.Add(this.nudServings);
            this.pnlMetadata.Controls.Add(this.lblCaloriesField);
            this.pnlMetadata.Controls.Add(this.nudTotalCalories);
            this.pnlMetadata.Controls.Add(this.lblDescriptionField);
            this.pnlMetadata.Controls.Add(this.txtDescription);

            // ---------- Tags kártya ----------
            ConfigureCard(this.pnlTags, 10, 360, 700, 95);

            this.lblTagsTitle.AutoSize  = true;
            this.lblTagsTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTagsTitle.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblTagsTitle.Location  = new System.Drawing.Point(20, 15);
            this.lblTagsTitle.Text      = "TAGEK";

            this.txtTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTags.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTags.Location    = new System.Drawing.Point(20, 45);
            this.txtTags.Size        = new System.Drawing.Size(650, 27);

            this.lblTagsHint.AutoSize  = true;
            this.lblTagsHint.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            this.lblTagsHint.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblTagsHint.Location  = new System.Drawing.Point(20, 76);
            this.lblTagsHint.Text      = "Címkék vesszővel elválasztva";

            this.pnlTags.Controls.Add(this.lblTagsTitle);
            this.pnlTags.Controls.Add(this.txtTags);
            this.pnlTags.Controls.Add(this.lblTagsHint);

            // ---------- Ingredients kártya ----------
            ConfigureCard(this.pnlIngredients, 10, 465, 700, 280);

            this.lblIngredientsTitle.AutoSize  = true;
            this.lblIngredientsTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblIngredientsTitle.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblIngredientsTitle.Location  = new System.Drawing.Point(20, 15);
            this.lblIngredientsTitle.Text      = "ÖSSZETEVŐK ÉS TERMÉK KAPCSOLÓDÁSOK";

            this.lblIngredientsHint.AutoSize  = true;
            this.lblIngredientsHint.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
            this.lblIngredientsHint.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblIngredientsHint.Location  = new System.Drawing.Point(420, 18);
            this.lblIngredientsHint.Text      = "← Húzz termékeket a katalógusból";

            this.dgvIngredients.AllowUserToAddRows       = false;
            this.dgvIngredients.AllowUserToResizeRows    = false;
            this.dgvIngredients.BackgroundColor          = System.Drawing.Color.White;
            this.dgvIngredients.BorderStyle              = System.Windows.Forms.BorderStyle.None;
            this.dgvIngredients.CellBorderStyle          = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvIngredients.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvIngredients.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 248, 246);
            this.dgvIngredients.ColumnHeadersDefaultCellStyle.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.dgvIngredients.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.dgvIngredients.ColumnHeadersHeight      = 32;
            this.dgvIngredients.EnableHeadersVisualStyles = false;
            this.dgvIngredients.Font                     = new System.Drawing.Font("Segoe UI", 10F);
            this.dgvIngredients.GridColor                = System.Drawing.Color.FromArgb(229, 229, 224);
            this.dgvIngredients.Location                 = new System.Drawing.Point(20, 45);
            this.dgvIngredients.RowHeadersVisible        = false;
            this.dgvIngredients.RowTemplate.Height       = 34;
            this.dgvIngredients.Size                     = new System.Drawing.Size(650, 220);
            this.dgvIngredients.AutoSizeColumnsMode      = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

            this.pnlIngredients.Controls.Add(this.lblIngredientsTitle);
            this.pnlIngredients.Controls.Add(this.lblIngredientsHint);
            this.pnlIngredients.Controls.Add(this.dgvIngredients);

            // ---------- Steps kártya ----------
            ConfigureCard(this.pnlSteps, 10, 755, 700, 180);

            this.lblStepsTitle.AutoSize  = true;
            this.lblStepsTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblStepsTitle.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblStepsTitle.Location  = new System.Drawing.Point(20, 15);
            this.lblStepsTitle.Text      = "ELKÉSZÍTÉS";

            this.txtSteps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSteps.Font        = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSteps.Location    = new System.Drawing.Point(20, 45);
            this.txtSteps.Multiline   = true;
            this.txtSteps.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSteps.Size        = new System.Drawing.Size(650, 120);

            this.pnlSteps.Controls.Add(this.lblStepsTitle);
            this.pnlSteps.Controls.Add(this.txtSteps);

            // ---------- Totals kártya ----------
            ConfigureCard(this.pnlTotals, 10, 945, 700, 70);

            this.lblTotalCost.AutoSize  = true;
            this.lblTotalCost.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalCost.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblTotalCost.Location  = new System.Drawing.Point(20, 22);
            this.lblTotalCost.Text      = "Becsült költség: 0 Ft";

            this.lblCostPerServing.AutoSize  = true;
            this.lblCostPerServing.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblCostPerServing.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblCostPerServing.Location  = new System.Drawing.Point(400, 25);
            this.lblCostPerServing.Text      = "";

            this.pnlTotals.Controls.Add(this.lblTotalCost);
            this.pnlTotals.Controls.Add(this.lblCostPerServing);

            // ---------- Preview URL kártya (a description alá pakolva a metadata kártyába) ----------
            ConfigureFieldLabel(this.lblPreviewUrlField, "ELŐNÉZETI KÉP URL", 20, 0);
            this.lblPreviewUrlField.Visible = false;
            this.txtPreviewImageUrl.Visible = false;

            // pnlEditor
            this.pnlEditor.AutoScroll = true;
            this.pnlEditor.BackColor  = System.Drawing.Color.FromArgb(248, 248, 246);
            this.pnlEditor.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditor.Controls.Add(this.pnlMetadata);
            this.pnlEditor.Controls.Add(this.pnlTags);
            this.pnlEditor.Controls.Add(this.pnlIngredients);
            this.pnlEditor.Controls.Add(this.pnlSteps);
            this.pnlEditor.Controls.Add(this.pnlTotals);

            // ===================================================================
            // SPLIT CONTAINER
            // ===================================================================

            this.splitMain.Dock             = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel       = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.SplitterDistance = 340;
            this.splitMain.SplitterWidth    = 6;
            this.splitMain.BackColor        = System.Drawing.Color.FromArgb(229, 229, 224);
            this.splitMain.Panel1.Controls.Add(this.pnlCatalog);
            this.splitMain.Panel2.Controls.Add(this.pnlEditor);

            // ===================================================================
            // LÁBLÉC
            // ===================================================================

            this.lblStatus.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Padding   = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.lblStatus.Text      = "Állapot: –";

            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 32;
            this.pnlFooter.Controls.Add(this.lblStatus);

            // ===================================================================
            // FORM
            // ===================================================================

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(248, 248, 246);
            this.ClientSize          = new System.Drawing.Size(1280, 820);
            this.Font                = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize         = new System.Drawing.Size(1100, 700);
            this.Name                = "MainForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "NaturaCo – Recept Szerkesztő";

            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);

            this.Load += new System.EventHandler(this.MainForm_Load);

            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlCatalog.ResumeLayout(false);
            this.pnlCatalog.PerformLayout();
            this.pnlMetadata.ResumeLayout(false);
            this.pnlMetadata.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrepTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCookTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudServings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalCalories)).EndInit();
            this.pnlTags.ResumeLayout(false);
            this.pnlTags.PerformLayout();
            this.pnlIngredients.ResumeLayout(false);
            this.pnlIngredients.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIngredients)).EndInit();
            this.pnlSteps.ResumeLayout(false);
            this.pnlSteps.PerformLayout();
            this.pnlTotals.ResumeLayout(false);
            this.pnlTotals.PerformLayout();
            this.pnlEditor.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // -------- Stílus segédek --------

        private static void ConfigureCard(System.Windows.Forms.Panel panel, int x, int y, int width, int height)
        {
            panel.BackColor   = System.Drawing.Color.White;
            panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel.Location    = new System.Drawing.Point(x, y);
            panel.Size        = new System.Drawing.Size(width, height);
        }

        private static void ConfigureFieldLabel(System.Windows.Forms.Label lbl, string text, int x, int y)
        {
            lbl.AutoSize  = true;
            lbl.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            lbl.Location  = new System.Drawing.Point(x, y);
            lbl.Text      = text;
        }

        private static void ConfigureFieldTextBox(System.Windows.Forms.TextBox tb, int x, int y, int width)
        {
            tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tb.Font        = new System.Drawing.Font("Segoe UI", 10F);
            tb.Location    = new System.Drawing.Point(x, y);
            tb.Size        = new System.Drawing.Size(width, 27);
        }

        private static void ConfigureFieldNumeric(System.Windows.Forms.NumericUpDown nud, int x, int y, int width, int min, int max)
        {
            nud.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            nud.Font        = new System.Drawing.Font("Segoe UI", 10F);
            nud.Location    = new System.Drawing.Point(x, y);
            nud.Size        = new System.Drawing.Size(width, 27);
            nud.Minimum     = min;
            nud.Maximum     = max;
        }

        private static void ConfigurePill(System.Windows.Forms.Label lbl, string text, int x, int y, System.Drawing.Color color)
        {
            lbl.AutoSize    = false;
            lbl.BackColor   = System.Drawing.Color.FromArgb(240, 240, 238);
            lbl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl.Font        = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor   = color;
            lbl.Location    = new System.Drawing.Point(x, y);
            lbl.Size        = new System.Drawing.Size(85, 24);
            lbl.Text        = text;
            lbl.TextAlign   = System.Drawing.ContentAlignment.MiddleCenter;
        }

        private static void ConfigurePrimaryButton(System.Windows.Forms.Button btn, string text, System.Drawing.Color color)
        {
            btn.BackColor               = color;
            btn.FlatStyle               = System.Windows.Forms.FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font                    = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btn.ForeColor               = System.Drawing.Color.White;
            btn.Text                    = text;
            btn.UseVisualStyleBackColor = false;
            btn.Cursor                  = System.Windows.Forms.Cursors.Hand;
        }
    }
}
