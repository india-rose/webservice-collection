using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;

namespace WebAPI.Controllers
{
	[RoutePrefix("/api/v1/devices")]
	[ApiAuthentification]
	public class DeviceController : ApiController
	{
		[Route("create")]
		[HttpPost]
		public HttpResponseMessage CreateDevice([FromBody] string name)
		{
			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.HasDevice(user, name))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Device name already exists");
				}
				
				database.CreateDevice(user, name);
				return Request.CreateResponse(HttpStatusCode.Created);
			}
		}
	}
}