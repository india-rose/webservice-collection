using System.Web.Http;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			// register route attribute
            config.MapHttpAttributeRoutes();
        }
    }
}
