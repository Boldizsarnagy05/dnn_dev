using DotNetNuke.Web.Api;

namespace NaturaCo.RecipeSyncApi.Infrastructure
{
    public sealed class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute(
                ModuleConstants.ModuleFolderName,
                "default",
                "{controller}/{action}",
                new { },
                new[] { ModuleConstants.ApiNamespace });
        }
    }
}
