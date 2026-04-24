using System.Net;
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
