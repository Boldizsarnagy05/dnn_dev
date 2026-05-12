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

        // ------------------------------------------------------------------
        // POST /DesktopModules/NaturaCo/API/RecipeSync/Save
        // ------------------------------------------------------------------
        [HttpPost]
        public HttpResponseMessage Save([FromBody] SaveRecipeRequest req)
        {
            if (req == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    new { Success = false, Message = "Ures request body." });

            try
            {
                int recipeId;

                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    if (req.RecipeId.HasValue && req.RecipeId.Value > 0)
                    {
                        // UPDATE
                        using (var cmd = new SqlCommand(@"
                            UPDATE dbo.RecipeRecipes SET
                                RecipeName       = @RecipeName,
                                ShortDescription = @ShortDescription,
                                Description      = @Description,
                                Category         = @MealType,
                                AuthorName       = @AuthorName,
                                PreviewImageURL  = @PreviewImageUrl,
                                Tags             = @Tags,
                                Servings         = @Servings,
                                PrepTimeMinutes  = @PrepTimeMinutes,
                                CookTimeMinutes  = @CookTimeMinutes,
                                TotalCalories    = @TotalCalories,
                                EstimatedCost    = @EstimatedCost,
                                Steps            = @Steps,
                                Status           = @Status,
                                CategoryBvin     = @CategoryBvin,
                                BundleBvin       = @BundleBvin
                            WHERE RecipeID = @RecipeId", conn))
                        {
                            AddRecipeParams(cmd, req);
                            cmd.Parameters.AddWithValue("@RecipeId", req.RecipeId.Value);
                            cmd.ExecuteNonQuery();
                        }

                        recipeId = req.RecipeId.Value;

                        // Hozzávalók törlése és újraszúrása
                        using (var del = new SqlCommand(
                            "DELETE FROM dbo.RecipeIngredients WHERE RecipeID = @id", conn))
                        {
                            del.Parameters.AddWithValue("@id", recipeId);
                            del.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // INSERT
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO dbo.RecipeRecipes
                                (RecipeName, ShortDescription, Description, Category,
                                 AuthorName, PreviewImageURL, Tags, Servings,
                                 PrepTimeMinutes, CookTimeMinutes, TotalCalories,
                                 EstimatedCost, Steps, Status, CategoryBvin, BundleBvin,
                                 PortalID, CreatedByUserID, CreatedOnDate)
                            VALUES
                                (@RecipeName, @ShortDescription, @Description, @MealType,
                                 @AuthorName, @PreviewImageUrl, @Tags, @Servings,
                                 @PrepTimeMinutes, @CookTimeMinutes, @TotalCalories,
                                 @EstimatedCost, @Steps, @Status, @CategoryBvin, @BundleBvin,
                                 0, -1, GETDATE());
                            SELECT SCOPE_IDENTITY();", conn))
                        {
                            AddRecipeParams(cmd, req);
                            recipeId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }

                    // Hozzávalók beszúrása
                    if (req.Ingredients != null)
                    {
                        foreach (var ing in req.Ingredients)
                        {
                            using (var ins = new SqlCommand(@"
                                INSERT INTO dbo.RecipeIngredients
                                    (RecipeID, IngredientName, Amount, Unit,
                                     ProductBvin, Calories, Price,
                                     PackageQuantity, PackageUnit, SortOrder)
                                VALUES
                                    (@RecipeID, @IngredientName, @Amount, @Unit,
                                     @ProductBvin, @Calories, @Price,
                                     @PackageQuantity, @PackageUnit, @SortOrder)", conn))
                            {
                                ins.Parameters.AddWithValue("@RecipeID",       recipeId);
                                ins.Parameters.AddWithValue("@IngredientName", ing.ProductName ?? string.Empty);
                                ins.Parameters.AddWithValue("@Amount",         ing.Quantity);
                                ins.Parameters.AddWithValue("@Unit",           ing.Unit ?? string.Empty);
                                ins.Parameters.AddWithValue("@ProductBvin",    (object)ing.ProductBvin ?? DBNull.Value);
                                ins.Parameters.AddWithValue("@Calories",       ing.Calories);
                                ins.Parameters.AddWithValue("@Price",          ing.Price);
                                ins.Parameters.AddWithValue("@PackageQuantity",ing.PackageQuantity);
                                ins.Parameters.AddWithValue("@PackageUnit",    ing.PackageUnit ?? string.Empty);
                                ins.Parameters.AddWithValue("@SortOrder",      ing.SortOrder);
                                ins.ExecuteNonQuery();
                            }
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success      = true,
                    RecipeId     = recipeId,
                    CategoryBvin = req.CategoryBvin ?? string.Empty,
                    BundleBvin   = req.BundleBvin   ?? string.Empty,
                    Status       = req.Status        ?? "Draft",
                    Message      = "Recept mentve."
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }

        private static void AddRecipeParams(SqlCommand cmd, SaveRecipeRequest req)
        {
            cmd.Parameters.AddWithValue("@RecipeName",       req.RecipeName       ?? string.Empty);
            cmd.Parameters.AddWithValue("@ShortDescription", req.ShortDescription ?? string.Empty);
            cmd.Parameters.AddWithValue("@Description",      req.Description      ?? string.Empty);
            cmd.Parameters.AddWithValue("@MealType",         req.MealType         ?? string.Empty);
            cmd.Parameters.AddWithValue("@AuthorName",       req.AuthorName       ?? string.Empty);
            cmd.Parameters.AddWithValue("@PreviewImageUrl",  req.PreviewImageUrl  ?? string.Empty);
            cmd.Parameters.AddWithValue("@Tags",             req.Tags             ?? string.Empty);
            cmd.Parameters.AddWithValue("@Servings",         req.Servings);
            cmd.Parameters.AddWithValue("@PrepTimeMinutes",  req.PrepTimeMinutes);
            cmd.Parameters.AddWithValue("@CookTimeMinutes",  req.CookTimeMinutes);
            cmd.Parameters.AddWithValue("@TotalCalories",    (object)req.TotalCalories ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstimatedCost",    req.EstimatedCost);
            cmd.Parameters.AddWithValue("@Steps",            req.Steps            ?? string.Empty);
            cmd.Parameters.AddWithValue("@Status",           req.Status           ?? "Draft");
            cmd.Parameters.AddWithValue("@CategoryBvin",     req.CategoryBvin     ?? string.Empty);
            cmd.Parameters.AddWithValue("@BundleBvin",       req.BundleBvin       ?? string.Empty);
        }
    }

    // DTO-k a Save vegponthoz
    public class SaveRecipeRequest
    {
        public int?   RecipeId         { get; set; }
        public string RecipeName       { get; set; }
        public string MealType         { get; set; }
        public string ShortDescription { get; set; }
        public string Description      { get; set; }
        public string Steps            { get; set; }
        public string Tags             { get; set; }
        public int    Servings         { get; set; }
        public int    PrepTimeMinutes  { get; set; }
        public int    CookTimeMinutes  { get; set; }
        public int?   TotalCalories    { get; set; }
        public decimal EstimatedCost   { get; set; }
        public string AuthorName       { get; set; }
        public string PreviewImageUrl  { get; set; }
        public string Status           { get; set; }
        public string CategoryBvin     { get; set; }
        public string BundleBvin       { get; set; }
        public bool   CreateOrUpdateBundle { get; set; }
        public bool   PublishAfterSave     { get; set; }
        public List<IngredientDto> Ingredients { get; set; }
    }

    public class IngredientDto
    {
        public string  ProductBvin     { get; set; }
        public string  ProductName     { get; set; }
        public decimal Quantity        { get; set; }
        public string  Unit            { get; set; }
        public int     SortOrder       { get; set; }
        public decimal Calories        { get; set; }
        public decimal Price           { get; set; }
        public decimal PackageQuantity { get; set; }
        public string  PackageUnit     { get; set; }
    }
}
