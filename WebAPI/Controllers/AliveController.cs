using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1")]
	public class AliveController : ApiController
	{
		/// <summary>
		/// Alive method to test if the api is online.
		/// </summary>
		/// <returns></returns>
		[Route("alive")]
		[HttpGet]
		[SwaggerResponse(HttpStatusCode.OK, "Ok if api is online")]
		public HttpResponseMessage Alive()
		{
			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}
