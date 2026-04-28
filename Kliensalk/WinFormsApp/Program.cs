using System;
using System.Configuration;
using System.Windows.Forms;
using NaturaCo.RecipeEditor.Forms;
using NaturaCo.RecipeEditor.Services;

namespace NaturaCo.RecipeEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Hotcakes katalogus bongeszesehez (csak olvasas)
            var storeUrl = ConfigurationManager.AppSettings["StoreUrl"];
            var apiKey   = ConfigurationManager.AppSettings["ApiKey"];

            // NaturaCo RecipeSync Save / Publish / Revoke
            var naturaCoApiUrl = ConfigurationManager.AppSettings["NaturaCoApiUrl"];

            if (string.IsNullOrWhiteSpace(naturaCoApiUrl))
            {
                MessageBox.Show(
                    "Az App.config-bol hianyzik a NaturaCoApiUrl kulcs.",
                    "Konfiguracios hiba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var recipeRootCategoryBvin = ConfigurationManager.AppSettings["RecipeRootCategoryBvin"] ?? string.Empty;

            var hccService    = new HotCakesService(storeUrl, apiKey);
            var recipeService = new RecipeApiService(naturaCoApiUrl);

            Application.Run(new MainForm(hccService, recipeService, recipeRootCategoryBvin));
        }
    }
}
