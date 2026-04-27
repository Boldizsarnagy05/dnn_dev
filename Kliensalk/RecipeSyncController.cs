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

                    // Fejadatok - minden tárolt mező
                    using (var cmd = new SqlCommand(@"
                        SELECT RecipeID, RecipeName, ShortDescription, Description, Category,
                               Servings, PrepTimeMinutes, CookTimeMinutes,
                               TotalCalories, EstimatedCost, Steps, Status,
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
                                Success          = true,
                                Message          = string.Empty,
                                RecipeId         = rdr.GetInt32(0),
                                RecipeName       = rdr.IsDBNull(1)  ? string.Empty : rdr.GetString(1),
                                ShortDescription = rdr.IsDBNull(2)  ? string.Empty : rdr.GetString(2),
                                Description      = rdr.IsDBNull(3)  ? string.Empty : rdr.GetString(3),
                                Category         = rdr.IsDBNull(4)  ? string.Empty : rdr.GetString(4),
                                Servings         = rdr.IsDBNull(5)  ? 1            : rdr.GetInt32(5),
                                PrepTime         = rdr.IsDBNull(6)  ? 0            : rdr.GetInt32(6),
                                CookTime         = rdr.IsDBNull(7)  ? 0            : rdr.GetInt32(7),
                                TotalCalories    = rdr.IsDBNull(8)  ? (int?)null   : rdr.GetInt32(8),
                                EstimatedCost    = rdr.IsDBNull(9)  ? (decimal?)null : rdr.GetDecimal(9),
                                Steps            = rdr.IsDBNull(10) ? string.Empty : rdr.GetString(10),
                                Status           = rdr.IsDBNull(11) ? "Draft"      : rdr.GetString(11),
                                CategoryBvin     = rdr.IsDBNull(12) ? string.Empty : rdr.GetString(12),
                                BundleBvin       = rdr.IsDBNull(13) ? string.Empty : rdr.GetString(13),
                            };
                        }
                    }

                    // Összetevők - ár és kalória adatokkal együtt
                    using (var cmd2 = new SqlCommand(@"
                        SELECT IngredientName, Amount, Unit, ProductBvin,
                               SortOrder, Calories, Price, PackageQuantity, PackageUnit
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
                                    ProductName     = rdr2.IsDBNull(0) ? string.Empty : rdr2.GetString(0),
                                    Quantity        = rdr2.IsDBNull(1) ? 0m           : rdr2.GetDecimal(1),
                                    Unit            = rdr2.IsDBNull(2) ? string.Empty : rdr2.GetString(2),
                                    ProductBvin     = rdr2.IsDBNull(3) ? string.Empty : rdr2.GetString(3),
                                    SortOrder       = rdr2.IsDBNull(4) ? 0            : rdr2.GetInt32(4),
                                    Calories        = rdr2.IsDBNull(5) ? 0m           : rdr2.GetDecimal(5),
                                    Price           = rdr2.IsDBNull(6) ? 0m           : rdr2.GetDecimal(6),
                                    PackageQuantity = rdr2.IsDBNull(7) ? 0m           : rdr2.GetDecimal(7),
                                    PackageUnit     = rdr2.IsDBNull(8) ? string.Empty : rdr2.GetString(8),
                                });
                            }
                        }
                    }
                }

                var json = JsonConvert.SerializeObject(new
                {
                    ((dynamic)recipe).Success,
                    ((dynamic)recipe).Message,
                    ((dynamic)recipe).RecipeId,
                    ((dynamic)recipe).RecipeName,
                    ((dynamic)recipe).ShortDescription,
                    ((dynamic)recipe).Description,
                    ((dynamic)recipe).Category,
                    ((dynamic)recipe).Servings,
                    ((dynamic)recipe).PrepTime,
                    ((dynamic)recipe).CookTime,
                    ((dynamic)recipe).TotalCalories,
                    ((dynamic)recipe).EstimatedCost,
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
