using System;
using System.Windows.Forms;
using NaturaCo.RecipeEditor.Services;

namespace NaturaCo.RecipeEditor.Forms
{
    public partial class LoginForm : Form
    {
        private readonly string _apiBaseUrl;

        public RecipeApiService AuthenticatedRecipeService { get; private set; }

        public LoginForm(string apiBaseUrl)
        {
            InitializeComponent();
            _apiBaseUrl = apiBaseUrl;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Kérjük add meg a felhasználónevet és a jelszót.");
                return;
            }

            btnLogin.Enabled = false;
            lblStatus.Text   = "Bejelentkezés folyamatban...";

            try
            {
                var service = new RecipeApiService(_apiBaseUrl);
                await service.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text);

                AuthenticatedRecipeService = service;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text   = "Bejelentkezés sikertelen.";
                btnLogin.Enabled = true;
                MessageBox.Show("Hiba: " + ex.Message);
            }
        }
    }
}
