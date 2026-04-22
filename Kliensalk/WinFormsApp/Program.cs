using System;
using System.Windows.Forms;
using NaturaCo.RecipeEditor.Forms;
using NaturaCo.RecipeEditor.Services;

namespace NaturaCo.RecipeEditor
{
    static class Program
    {
        // HotCakes REST API konfiguráció
        // Az API key a HotCakes Admin → Configuration → API oldalon található
        private const string StoreUrl = "https://naturaco.hu";
        private const string ApiKey   = "1-e0926313-2fa4-4a60-8b66-0288743497d5";

        // Saját DNN Web API alap URL
        private const string DnnApiUrl = "https://naturaco.hu";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Bejelentkezési ablak
            using (var loginForm = new LoginForm(DnnApiUrl))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                    return;

                // Szolgáltatások inicializálása bejelentkezés után
                var hccService    = new HotCakesService(StoreUrl, ApiKey);
                var recipeService = loginForm.AuthenticatedRecipeService;

                Application.Run(new MainForm(hccService, recipeService));
            }
        }
    }
}
