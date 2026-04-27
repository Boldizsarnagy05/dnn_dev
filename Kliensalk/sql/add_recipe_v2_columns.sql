-- ==============================================================
-- RecipeRecipes és RecipeIngredients bővítése v2 mezőkkel
-- Futtatás: egyszer, az API v2 beüzemelése előtt/után
-- Előfeltétel: create_recipe_tables.sql + add_recipe_sync_columns.sql már lefutott
-- ==============================================================
SET NOCOUNT ON;

-- --------------------------------------------------------------
-- RecipeRecipes: ShortDescription, EstimatedCost
-- (CategoryBvin, BundleBvin már megvan az előző migrációból)
-- --------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeRecipes' AND COLUMN_NAME = 'ShortDescription'
)
    ALTER TABLE [dbo].[RecipeRecipes] ADD [ShortDescription] NVARCHAR(500) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeRecipes' AND COLUMN_NAME = 'EstimatedCost'
)
    ALTER TABLE [dbo].[RecipeRecipes] ADD [EstimatedCost] DECIMAL(10,2) NULL;

PRINT 'RecipeRecipes bővítve: ShortDescription, EstimatedCost';

-- --------------------------------------------------------------
-- RecipeIngredients: ProductBvin (string), Calories, Price,
--                    PackageQuantity, PackageUnit
-- Megjegyzés: ProductID (UNIQUEIDENTIFIER) megmarad visszafelé
-- kompatibilitáshoz; ProductBvin az API által küldött string bvin.
-- --------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeIngredients' AND COLUMN_NAME = 'ProductBvin'
)
    ALTER TABLE [dbo].[RecipeIngredients] ADD [ProductBvin] NVARCHAR(50) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeIngredients' AND COLUMN_NAME = 'Calories'
)
    ALTER TABLE [dbo].[RecipeIngredients] ADD [Calories] DECIMAL(10,2) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeIngredients' AND COLUMN_NAME = 'Price'
)
    ALTER TABLE [dbo].[RecipeIngredients] ADD [Price] DECIMAL(10,2) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeIngredients' AND COLUMN_NAME = 'PackageQuantity'
)
    ALTER TABLE [dbo].[RecipeIngredients] ADD [PackageQuantity] DECIMAL(10,2) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'RecipeIngredients' AND COLUMN_NAME = 'PackageUnit'
)
    ALTER TABLE [dbo].[RecipeIngredients] ADD [PackageUnit] NVARCHAR(50) NULL;

-- Meglévő soroknál a ProductBvin-t feltöltjük ProductID-ból (ahol van)
UPDATE [dbo].[RecipeIngredients]
SET    [ProductBvin] = CAST([ProductID] AS NVARCHAR(50))
WHERE  [ProductBvin] IS NULL
  AND  [ProductID]  IS NOT NULL;

PRINT 'RecipeIngredients bővítve: ProductBvin, Calories, Price, PackageQuantity, PackageUnit';
PRINT 'Migráció kész.';
