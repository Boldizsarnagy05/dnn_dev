/*
' Copyright (c) 2026 Boldizsár Nagy
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Web.Mvc;
using Teszko.ReceptModulRecept_modul.Components;
using Teszko.ReceptModulRecept_modul.Models;

namespace Teszko.ReceptModulRecept_modul.Controllers
{
    [DnnHandleError]
    public class ItemController : DnnController
    {
        private readonly IRecipeStorefrontService _recipes;

        public ItemController()
            : this(new RecipeStorefrontService())
        {
        }

        internal ItemController(IRecipeStorefrontService recipes)
        {
            _recipes = recipes;
        }

        public ActionResult Details(string id)
        {
            var recipe = _recipes.GetRecipe(id);
            if (recipe == null)
            {
                return RedirectToDefaultRoute();
            }

            return View(recipe);
        }

        public ActionResult Index(string mealType = "all")
        {
            return View(_recipes.GetRecipes(mealType));
        }

        [HttpPost]
        public JsonResult AddProductsToCart(AddRecipeCartRequest request)
        {
            var result = _recipes.AddToCart(request?.Items);
            return Json(result);
        }
    }
}
