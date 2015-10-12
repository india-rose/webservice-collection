using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/devices")]
	[ApiAuthentification]
	public class DeviceController : ApiController
	{
		[Route("create")]
		[HttpPost]
		public HttpResponseMessage CreateDevice([FromBody] string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return Request.CreateBadRequestResponse();
			}

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

		[Route("rename")]
		[HttpPost]
		public HttpResponseMessage RenameDevice([FromBody] string oldName, string newName)
		{
			if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
			{
				return Request.CreateBadRequestResponse();
			}

			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				if (!database.UpdateDevice(user, oldName, newName))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Device name not found");
				}
				return Request.CreateResponse(HttpStatusCode.Accepted);
			}
		}

		[Route("list")]
		[HttpGet]
		public HttpResponseMessage ListDevices()
		{
			User user = RequestContext.GetAuthenticatedUser();

			using(IDatabaseService database = new DatabaseService())
			{
				List<Device> devices = database.GetDevices(user).ToList();

				return Request.CreateGoodReponse(devices.Select(x => new DeviceResponse(x.DeviceName)).ToList());
			}
		}
	}
}