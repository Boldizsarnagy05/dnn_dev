namespace NaturaCo.RecipeEditor.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlCard      = new System.Windows.Forms.Panel();
            this.lblBrand     = new System.Windows.Forms.Label();
            this.lblSubtitle  = new System.Windows.Forms.Label();
            this.lblUsername  = new System.Windows.Forms.Label();
            this.txtUsername  = new System.Windows.Forms.TextBox();
            this.lblPassword  = new System.Windows.Forms.Label();
            this.txtPassword  = new System.Windows.Forms.TextBox();
            this.btnLogin     = new System.Windows.Forms.Button();
            this.lblStatus    = new System.Windows.Forms.Label();

            this.pnlCard.SuspendLayout();
            this.SuspendLayout();

            // lblBrand
            this.lblBrand.AutoSize  = true;
            this.lblBrand.Font      = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.FromArgb(44, 44, 44);
            this.lblBrand.Location  = new System.Drawing.Point(30, 28);
            this.lblBrand.Text      = "NaturaCo";

            // lblSubtitle
            this.lblSubtitle.AutoSize  = true;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblSubtitle.Location  = new System.Drawing.Point(32, 72);
            this.lblSubtitle.Text      = "Recept szerkesztő – jelentkezz be a folytatáshoz";

            // lblUsername
            this.lblUsername.AutoSize  = true;
            this.lblUsername.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblUsername.Location  = new System.Drawing.Point(32, 130);
            this.lblUsername.Text      = "FELHASZNÁLÓNÉV";

            // txtUsername
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsername.Font        = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUsername.Location    = new System.Drawing.Point(32, 152);
            this.txtUsername.Size        = new System.Drawing.Size(286, 27);

            // lblPassword
            this.lblPassword.AutoSize  = true;
            this.lblPassword.Font      = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblPassword.Location  = new System.Drawing.Point(32, 200);
            this.lblPassword.Text      = "JELSZÓ";

            // txtPassword
            this.txtPassword.BorderStyle           = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font                  = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPassword.Location              = new System.Drawing.Point(32, 222);
            this.txtPassword.Size                  = new System.Drawing.Size(286, 27);
            this.txtPassword.UseSystemPasswordChar = true;

            // btnLogin
            this.btnLogin.BackColor                = System.Drawing.Color.FromArgb(107, 142, 35);
            this.btnLogin.FlatStyle                = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Font                     = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor                = System.Drawing.Color.White;
            this.btnLogin.Location                 = new System.Drawing.Point(32, 278);
            this.btnLogin.Size                     = new System.Drawing.Size(286, 44);
            this.btnLogin.Text                     = "Bejelentkezés";
            this.btnLogin.UseVisualStyleBackColor  = false;
            this.btnLogin.Cursor                   = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.Click                   += new System.EventHandler(this.btnLogin_Click);

            // lblStatus
            this.lblStatus.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(136, 136, 136);
            this.lblStatus.Location  = new System.Drawing.Point(32, 338);
            this.lblStatus.Size      = new System.Drawing.Size(286, 36);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // pnlCard
            this.pnlCard.BackColor   = System.Drawing.Color.White;
            this.pnlCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard.Location    = new System.Drawing.Point(40, 40);
            this.pnlCard.Size        = new System.Drawing.Size(350, 400);
            this.pnlCard.Controls.Add(this.lblBrand);
            this.pnlCard.Controls.Add(this.lblSubtitle);
            this.pnlCard.Controls.Add(this.lblUsername);
            this.pnlCard.Controls.Add(this.txtUsername);
            this.pnlCard.Controls.Add(this.lblPassword);
            this.pnlCard.Controls.Add(this.txtPassword);
            this.pnlCard.Controls.Add(this.btnLogin);
            this.pnlCard.Controls.Add(this.lblStatus);

            // LoginForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(248, 248, 246);
            this.ClientSize          = new System.Drawing.Size(430, 480);
            this.Font                = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox         = false;
            this.Name                = "LoginForm";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "NaturaCo Recept Szerkesztő – Bejelentkezés";
            this.AcceptButton        = this.btnLogin;
            this.Controls.Add(this.pnlCard);

            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
