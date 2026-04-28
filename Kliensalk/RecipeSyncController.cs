// DesktopModules/NaturaCo/Controllers/RecipeSyncController.cs
//
// DNN Service Framework Web API végpontok a Recept szerkesztőhöz.
// URL séma: /DesktopModules/NaturaCo/API/RecipeSync/{action}
//
// Telepítés: a lefordított DLL a DesktopModules/NaturaCo/bin/ mappába kerül.
// Előfeltétel: create_recipe_tables.sql + add_recipe_sync_columns.sql
//              + add_recipe_v2_columns.sql lefutott.

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
        // ------------------------------------------------------------------
        [HttpGet]
        public HttpResponseMessage List()
        {
            try
            {
                var list = new List<object>();

                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"
                    SELECT RecipeID, RecipeName, Status,
                           ISNULL(CategoryBvin, '')  AS CategoryBvin,
                           ISNULL(BundleBvin,   '')  AS BundleBvin,
                           ISNULL(Category,     '')  AS MealType,
                           ISNULL(Servings,     1)   AS Servings,
                           ISNULL(PrepTimeMinutes, 0) AS PrepTimeMinutes,
                           ISNULL(CookTimeMinutes, 0) AS CookTimeMinutes,
                           TotalCalories,
                           ISNULL(ShortDescription, '') AS ShortDescription
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
                                RecipeId         = rdr.GetInt32(0),
                                RecipeName       = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1),
                                Status           = rdr.IsDBNull(2) ? "Draft"      : rdr.GetString(2),
                                CategoryBvin     = rdr.GetString(3),
                                BundleBvin       = rdr.GetString(4),
                                MealType         = rdr.GetString(5),
                                Servings         = rdr.GetInt32(6),
                                PrepTimeMinutes  = rdr.GetInt32(7),
                                CookTimeMinutes  = rdr.GetInt32(8),
                                TotalCalories    = rdr.IsDBNull(9) ? (int?)null : rdr.GetInt32(9),
                                ShortDescription = rdr.GetString(10),
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
                        SELECT RecipeID, RecipeName,
                               ISNULL(ShortDescription, '') AS ShortDescription,
                               ISNULL(Description, '')      AS Description,
                               ISNULL(Category, '')         AS Category,
                               ISNULL(Servings, 1)          AS Servings,
                               ISNULL(PrepTimeMinutes, 0)   AS PrepTimeMinutes,
                               ISNULL(CookTimeMinutes, 0)   AS CookTimeMinutes,
                               TotalCalories,
                               EstimatedCost,
                               ISNULL(Steps, '')            AS Steps,
                               ISNULL(Status, 'Draft')      AS Status,
                               ISNULL(CategoryBvin, '')     AS CategoryBvin,
                               ISNULL(BundleBvin, '')       AS BundleBvin
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
                                RecipeName       = rdr.GetString(1),
                                ShortDescription = rdr.GetString(2),
                                Description      = rdr.GetString(3),
                                Category         = rdr.GetString(4),
                                Servings         = rdr.GetInt32(5),
                                PrepTime         = rdr.GetInt32(6),
                                CookTime         = rdr.GetInt32(7),
                                TotalCalories    = rdr.IsDBNull(8)  ? (int?)null     : rdr.GetInt32(8),
                                EstimatedCost    = rdr.IsDBNull(9)  ? (decimal?)null : rdr.GetDecimal(9),
                                Steps            = rdr.GetString(10),
                                Status           = rdr.GetString(11),
                                CategoryBvin     = rdr.GetString(12),
                                BundleBvin       = rdr.GetString(13),
                            };
                        }
                    }

                    // Összetevők
                    // ProductBvin: elsősorban a string oszlopból, fallback: ProductID cast-olva
                    using (var cmd2 = new SqlCommand(@"
                        SELECT IngredientName,
                               ISNULL(Amount, 0)          AS Amount,
                               ISNULL(Unit, '')           AS Unit,
                               ISNULL(
                                   ProductBvin,
                                   CAST(ProductID AS NVARCHAR(50))
                               )                          AS ProductBvin,
                               ISNULL(SortOrder, 0)       AS SortOrder,
                               ISNULL(Calories, 0)        AS Calories,
                               ISNULL(Price, 0)           AS Price,
                               ISNULL(PackageQuantity, 0) AS PackageQuantity,
                               ISNULL(PackageUnit, '')    AS PackageUnit
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
                                    Quantity        = rdr2.GetDecimal(1),
                                    Unit            = rdr2.GetString(2),
                                    ProductBvin     = rdr2.IsDBNull(3) ? string.Empty : rdr2.GetString(3),
                                    SortOrder       = rdr2.GetInt32(4),
                                    Calories        = rdr2.GetDecimal(5),
                                    Price           = rdr2.GetDecimal(6),
                                    PackageQuantity = rdr2.GetDecimal(7),
                                    PackageUnit     = rdr2.GetString(8),
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
