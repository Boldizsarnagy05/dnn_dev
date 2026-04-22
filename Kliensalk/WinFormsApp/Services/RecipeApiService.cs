using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NaturaCo.RecipeEditor.Models;

namespace NaturaCo.RecipeEditor.Services
{
    // Saját DNN Web API hívások – recept CRUD és publikálás
    // Alap URL: https://naturaco.hu/api/RecipeModule
    public class RecipeApiService
    {
        private readonly HttpClient _http;
        private readonly string     _baseUrl;

        public RecipeApiService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http    = new HttpClient();
            _http.DefaultRequestHeaders.Accept
                 .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ------------------------------------------------------------------
        // Autentikáció – DNN JWT token
        // ------------------------------------------------------------------

        public async Task LoginAsync(string username, string password)
        {
            var payload = JsonConvert.SerializeObject(new { u = username, p = password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(
                $"{_baseUrl}/api/JWT/Login", content);

            response.EnsureSuccessStatusCode();

            var json  = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<dynamic>(json);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", (string)token.accessToken);
        }

        // ------------------------------------------------------------------
        // Recept CRUD
        // ------------------------------------------------------------------

        public async Task<List<Recipe>> GetRecipesAsync()
        {
            var response = await _http.GetAsync($"{_baseUrl}/Recipe");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Recipe>>(json);
        }

        public async Task<Recipe> GetRecipeAsync(int recipeId)
        {
            var response = await _http.GetAsync($"{_baseUrl}/Recipe/{recipeId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Recipe>(json);
        }

        public async Task<Recipe> SaveRecipeAsync(Recipe recipe)
        {
            var payload = JsonConvert.SerializeObject(recipe);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (recipe.RecipeID == 0)
            {
                // Új recept
                response = await _http.PostAsync($"{_baseUrl}/Recipe", content);
            }
            else
            {
                // Meglévő recept frissítése
                response = await _http.PutAsync($"{_baseUrl}/Recipe/{recipe.RecipeID}", content);
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Recipe>(json);
        }

        public async Task DeleteRecipeAsync(int recipeId)
        {
            var response = await _http.DeleteAsync($"{_baseUrl}/Recipe/{recipeId}");
            response.EnsureSuccessStatusCode();
        }

        // ------------------------------------------------------------------
        // Publikálás – státusz váltás Draft → Published
        // A bundle létrehozás itt történne meg, de egyelőre kommentálva van
        // ------------------------------------------------------------------

        public async Task<Recipe> PublishRecipeAsync(int recipeId)
        {
            // Státusz frissítés Published-re
            var response = await _http.PostAsync(
                $"{_baseUrl}/Recipe/{recipeId}/publish", null);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Recipe>(json);

            // Bundle létrehozás – EGYELŐRE KOMMENTÁLVA
            // Akkor aktiválható, ha a saját DNN Web API bundle végpontja elkészül
            // A HotCakes REST API nem támogatja a bundle műveletet (nincs IsBundle a DTO-ban)
            //
            // await _http.PostAsync($"{_baseUrl}/Recipe/{recipeId}/bundle", null);
        }

        // ------------------------------------------------------------------
        // Visszavonás – Published → Revoked
        // ------------------------------------------------------------------

        public async Task RevokeRecipeAsync(int recipeId)
        {
            var response = await _http.PostAsync(
                $"{_baseUrl}/Recipe/{recipeId}/revoke", null);

            response.EnsureSuccessStatusCode();

            // Bundle inaktiválás – EGYELŐRE KOMMENTÁLVA
            //
            // await _http.DeleteAsync($"{_baseUrl}/Recipe/{recipeId}/bundle");
        }
    }
}
