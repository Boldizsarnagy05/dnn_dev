using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using NaturaCo.RecipeSyncApi.Models;
using NaturaCo.RecipeSyncApi.Services;

namespace NaturaCo.RecipeSyncApi.Controllers
{
    [AllowAnonymous]
    public sealed class RecipeSyncController : DnnApiController
    {
        private readonly IRecipeSyncService _service;

        public RecipeSyncController()
            : this(new RecipeSyncService())
        {
        }

        public RecipeSyncController(IRecipeSyncService service)
        {
            _service = service;
        }

        [HttpGet]
        public HttpResponseMessage List()
        {
            return Request.CreateResponse(HttpStatusCode.OK, _service.List());
        }

        [HttpGet]
        public HttpResponseMessage Load(int id)
        {
            var recipe = _service.Load(id);
            if (recipe == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new
                {
                    Success = false,
                    Message = "A recept nem talalhato."
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Success = true,
                Recipe = recipe
            });
        }

        [HttpPost]
        public IHttpActionResult Save(SaveRecipeRequest request)
        {
            var result = _service.Save(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return Content(HttpStatusCode.BadRequest, result);
        }

        [HttpPost]
        public IHttpActionResult Publish(PublishRecipeRequest request)
        {
            var result = _service.Publish(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return Content(HttpStatusCode.BadRequest, result);
        }

        [HttpPost]
        public IHttpActionResult Revoke(RevokeRecipeRequest request)
        {
            var result = _service.Revoke(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return Content(HttpStatusCode.BadRequest, result);
        }
    }
}
