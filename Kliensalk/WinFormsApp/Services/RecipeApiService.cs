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
    // NaturaCo DNN modul HTTP klienseje.
    //
    // Vegpontok (CLIENT_APP_CONTEXT.md):
    //   POST {baseUrl}/DesktopModules/NaturaCo/API/RecipeSync/Save
    //   POST {baseUrl}/DesktopModules/NaturaCo/API/RecipeSync/Publish
    //   POST {baseUrl}/DesktopModules/NaturaCo/API/RecipeSync/Revoke
    //
    // A controller jelenleg AllowAnonymous, ezert sima HTTP POST eleg, nincs
    // sem RequestVerificationToken, sem auth header.
    public class RecipeApiService
    {
        private const string BasePath = "/DesktopModules/NaturaCo/API/RecipeSync";

        private readonly HttpClient _http;
        private readonly string     _baseUrl;

        public RecipeApiService(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("NaturaCoApiUrl nincs konfiguralva.", nameof(baseUrl));

            _baseUrl = baseUrl.TrimEnd('/');
            _http    = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.Accept
                 .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ------------------------------------------------------------------
        // Save / Publish / Revoke
        // ------------------------------------------------------------------

        public Task<RecipeSyncResult> SaveAsync(SaveRecipeRequest request)
        {
            return PostJsonAsync("/Save", request);
        }

        public Task<RecipeSyncResult> PublishAsync(PublishRecipeRequest request)
        {
            return PostJsonAsync("/Publish", request);
        }

        public Task<RecipeSyncResult> RevokeAsync(RevokeRecipeRequest request)
        {
            return PostJsonAsync("/Revoke", request);
        }

        // ------------------------------------------------------------------
        // Lista / Betoltes
        // ------------------------------------------------------------------

        public async Task<List<RecipeListItem>> GetRecipesAsync()
        {
            try
            {
                var url  = _baseUrl + BasePath + "/List";
                var json = await _http.GetStringAsync(url);
                return JsonConvert.DeserializeObject<List<RecipeListItem>>(json)
                       ?? new List<RecipeListItem>();
            }
            catch
            {
                return new List<RecipeListItem>();
            }
        }

        public async Task<RecipeLoadResult> LoadRecipeAsync(int recipeId)
        {
            try
            {
                var url  = _baseUrl + BasePath + "/Load?id=" + recipeId;
                var json = await _http.GetStringAsync(url);
                return JsonConvert.DeserializeObject<RecipeLoadResult>(json)
                       ?? new RecipeLoadResult { Success = false, Message = "Ures szerver-valasz." };
            }
            catch (Exception ex)
            {
                return new RecipeLoadResult { Success = false, Message = ex.Message };
            }
        }

        // ------------------------------------------------------------------
        // Belso segedek
        // ------------------------------------------------------------------

        private async Task<RecipeSyncResult> PostJsonAsync(string action, object body)
        {
            var url     = _baseUrl + BasePath + action;
            var json    = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                // Halozati hiba - egysegesen RecipeSyncResult-ben adjuk vissza,
                // hogy a UI ugyanazt a Message/Errors mezot tudja megjeleniteni.
                return new RecipeSyncResult
                {
                    Success = false,
                    Message = "A szerver nem elerheto: " + ex.Message
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            // 200 vagy 400 eseten is RecipeSyncResult-et varunk
            // (lasd CLIENT_APP_CONTEXT.md "Hibas valaszok" reszt).
            RecipeSyncResult result = null;
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try { result = JsonConvert.DeserializeObject<RecipeSyncResult>(responseBody); }
                catch { /* nem JSON - eldobjuk */ }
            }

            if (result != null)
                return result;

            // Nem ertelmezheto valasz - csomagoljunk be egy hibaobjektumot.
            return new RecipeSyncResult
            {
                Success = false,
                Message = $"Varatlan szerver-valasz (HTTP {(int)response.StatusCode}).",
                Errors  = { string.IsNullOrWhiteSpace(responseBody) ? "(ures body)" : responseBody }
            };
        }
    }
}
