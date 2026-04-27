// DesktopModules/NaturaCo/Controllers/RecipeSyncController.cs
//
// DNN Service Framework Web API végpontok a Recept szerkesztőhöz.
// URL séma: /DesktopModules/NaturaCo/API/RecipeSync/{action}
//
// Telepítés: a lefordított DLL a DesktopModules/NaturaCo/bin/ mappába kerül.
// Előfeltétel: add_recipe_sync_columns.sql lefutott (CategoryBvin, BundleBvin oszlopok).

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;

namespace NaturaCo.Modules.RecipeModule.Controllers
{
    [AllowAnonymous]
    public class RecipeSyncController : DnnApiController
    {
        private static readonly string ConnStr =
            System.Configuration.ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;

        // ------------------------------------------------------------------
        // GET /DesktopModules/NaturaCo/API/RecipeSync/List
        // Visszaad minden receptet (RecipeID, RecipeName, Status, CategoryBvin, BundleBvin)
        // ------------------------------------------------------------------
        [HttpGet]
        public HttpResponseMessage List()
        {
            try
            {
                var list = new List<object>();

                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"
                    SELECT RecipeID, RecipeName, Status, CategoryBvin, BundleBvin
                    FROM   dbo.RecipeRecipes
                    ORDER  BY RecipeName", conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new
                            {
                                RecipeId     = rdr.GetInt32(0),
                                RecipeName   = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1),
                                Status       = rdr.IsDBNull(2) ? "Draft"     : rdr.GetString(2),
                                CategoryBvin = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3),
                                BundleBvin   = rdr.IsDBNull(4) ? string.Empty : rdr.GetString(4),
                            });
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }

        // ------------------------------------------------------------------
        // GET /DesktopModules/NaturaCo/API/RecipeSync/Load?id={id}
        // Teljes recept betöltése szerkesztéshez (fejadatok + összetevők)
        // ------------------------------------------------------------------
        [HttpGet]
        public HttpResponseMessage Load(int id)
        {
            try
            {
                object recipe = null;
                var ingredients = new List<object>();

                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    // Fejadatok
                    using (var cmd = new SqlCommand(@"
                        SELECT RecipeID, RecipeName, Description, Category,
                               Servings, PrepTimeMinutes, CookTimeMinutes,
                               TotalCalories, Steps, Status,
                               CategoryBvin, BundleBvin
                        FROM   dbo.RecipeRecipes
                        WHERE  RecipeID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read())
                                return Request.CreateResponse(HttpStatusCode.NotFound,
                                    new { Success = false, Message = "A recept nem található." });

                            recipe = new
                            {
                                Success      = true,
                                Message      = string.Empty,
                                RecipeId     = rdr.GetInt32(0),
                                RecipeName   = rdr.IsDBNull(1)  ? string.Empty : rdr.GetString(1),
                                Description  = rdr.IsDBNull(2)  ? string.Empty : rdr.GetString(2),
                                Category     = rdr.IsDBNull(3)  ? string.Empty : rdr.GetString(3),
                                Servings     = rdr.IsDBNull(4)  ? 1            : rdr.GetInt32(4),
                                PrepTime     = rdr.IsDBNull(5)  ? 0            : rdr.GetInt32(5),
                                CookTime     = rdr.IsDBNull(6)  ? 0            : rdr.GetInt32(6),
                                TotalCalories= rdr.IsDBNull(7)  ? (int?)null   : rdr.GetInt32(7),
                                Steps        = rdr.IsDBNull(8)  ? string.Empty : rdr.GetString(8),
                                Status       = rdr.IsDBNull(9)  ? "Draft"      : rdr.GetString(9),
                                CategoryBvin = rdr.IsDBNull(10) ? string.Empty : rdr.GetString(10),
                                BundleBvin   = rdr.IsDBNull(11) ? string.Empty : rdr.GetString(11),
                            };
                        }
                    }

                    // Összetevők
                    using (var cmd2 = new SqlCommand(@"
                        SELECT IngredientName, Amount, Unit,
                               CAST(ProductID AS NVARCHAR(50)) AS ProductBvin,
                               SortOrder
                        FROM   dbo.RecipeIngredients
                        WHERE  RecipeID = @id
                        ORDER  BY SortOrder", conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", id);
                        using (var rdr2 = cmd2.ExecuteReader())
                        {
                            while (rdr2.Read())
                            {
                                ingredients.Add(new
                                {
                                    ProductName = rdr2.IsDBNull(0) ? string.Empty : rdr2.GetString(0),
                                    Quantity    = rdr2.IsDBNull(1) ? 0m           : rdr2.GetDecimal(1),
                                    Unit        = rdr2.IsDBNull(2) ? string.Empty : rdr2.GetString(2),
                                    ProductBvin = rdr2.IsDBNull(3) ? string.Empty : rdr2.GetString(3),
                                    SortOrder   = rdr2.IsDBNull(4) ? 0            : rdr2.GetInt32(4),
                                });
                            }
                        }
                    }
                }

                // Összerakjuk a végső választ (Ingredients dinamikusan kerül bele)
                var json = JsonConvert.SerializeObject(new
                {
                    ((dynamic)recipe).Success,
                    ((dynamic)recipe).Message,
                    ((dynamic)recipe).RecipeId,
                    ((dynamic)recipe).RecipeName,
                    ((dynamic)recipe).Description,
                    ((dynamic)recipe).Category,
                    ((dynamic)recipe).Servings,
                    ((dynamic)recipe).PrepTime,
                    ((dynamic)recipe).CookTime,
                    ((dynamic)recipe).TotalCalories,
                    ((dynamic)recipe).Steps,
                    ((dynamic)recipe).Status,
                    ((dynamic)recipe).CategoryBvin,
                    ((dynamic)recipe).BundleBvin,
                    Ingredients = ingredients
                });

                var resp = Request.CreateResponse(HttpStatusCode.OK);
                resp.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return resp;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }
    }
}
