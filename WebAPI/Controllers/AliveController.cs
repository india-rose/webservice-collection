using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1")]
	public class AliveController : ApiController
	{
		[Route("alive")]
		[HttpGet]
		public HttpResponseMessage Alive()
		{
			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}
