namespace NaturaCo.RecipeEditor.Forms
{
    partial class CustomIngredientDialog
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label          lblName;
        private System.Windows.Forms.TextBox        txtName;
        private System.Windows.Forms.Label          lblAmount;
        private System.Windows.Forms.NumericUpDown  nudAmount;
        private System.Windows.Forms.Label          lblUnit;
        private System.Windows.Forms.ComboBox       cmbUnit;
        private System.Windows.Forms.Button         btnOk;
        private System.Windows.Forms.Button         btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblName   = new System.Windows.Forms.Label();
            this.txtName   = new System.Windows.Forms.TextBox();
            this.lblAmount = new System.Windows.Forms.Label();
            this.nudAmount = new System.Windows.Forms.NumericUpDown();
            this.lblUnit   = new System.Windows.Forms.Label();
            this.cmbUnit   = new System.Windows.Forms.ComboBox();
            this.btnOk     = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudAmount)).BeginInit();
            this.SuspendLayout();

            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(16, 18);
            this.lblName.Text     = "Hozzávaló neve:";

            // txtName
            this.txtName.Location = new System.Drawing.Point(16, 40);
            this.txtName.Size     = new System.Drawing.Size(340, 36);
            this.txtName.TabIndex = 0;
            this.txtName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtName_KeyDown);

            // lblAmount
            this.lblAmount.AutoSize = true;
            this.lblAmount.Location = new System.Drawing.Point(16, 92);
            this.lblAmount.Text     = "Mennyiség:";

            // nudAmount
            this.nudAmount.Location      = new System.Drawing.Point(16, 114);
            this.nudAmount.Size          = new System.Drawing.Size(160, 36);
            this.nudAmount.TabIndex      = 1;
            this.nudAmount.DecimalPlaces = 2;
            this.nudAmount.Minimum       = new decimal(new int[] { 0, 0, 0, 0 });
            this.nudAmount.Maximum       = new decimal(new int[] { 10000, 0, 0, 0 });
            this.nudAmount.Value         = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudAmount.Increment     = new decimal(new int[] { 1, 0, 0, 0 });

            // lblUnit
            this.lblUnit.AutoSize = true;
            this.lblUnit.Location = new System.Drawing.Point(196, 92);
            this.lblUnit.Text     = "Egység:";

            // cmbUnit
            this.cmbUnit.Location = new System.Drawing.Point(196, 114);
            this.cmbUnit.Size     = new System.Drawing.Size(160, 36);
            this.cmbUnit.TabIndex = 2;
            this.cmbUnit.Items.AddRange(new object[] {
                "g", "dkg", "kg", "ml", "dl", "l",
                "db", "ek", "tk", "csipet", "ízlés szerint"
            });
            this.cmbUnit.Text = "g";

            // btnOk
            this.btnOk.Location    = new System.Drawing.Point(196, 170);
            this.btnOk.Size        = new System.Drawing.Size(160, 40);
            this.btnOk.TabIndex    = 3;
            this.btnOk.Text        = "Hozzáadás";
            this.btnOk.Click      += new System.EventHandler(this.btnOk_Click);

            // btnCancel
            this.btnCancel.Location    = new System.Drawing.Point(16, 170);
            this.btnCancel.Size        = new System.Drawing.Size(160, 40);
            this.btnCancel.TabIndex    = 4;
            this.btnCancel.Text        = "Mégse";
            this.btnCancel.Click      += new System.EventHandler(this.btnCancel_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(374, 228);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.nudAmount);
            this.Controls.Add(this.lblUnit);
            this.Controls.Add(this.cmbUnit);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "CustomIngredientDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Egyedi hozzávaló";
            ((System.ComponentModel.ISupportInitialize)(this.nudAmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
