using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Common.Requests;
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
		public HttpResponseMessage CreateDevice([FromBody] DeviceCreateRequest device)
		{
			if (string.IsNullOrWhiteSpace(device.Name))
			{
				return Request.CreateBadRequestResponse();
			}

			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.HasDevice(user, device.Name))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Device name already exists");
				}
				
				database.CreateDevice(user, device.Name);
				return Request.CreateResponse(HttpStatusCode.Created);
			}
		}

		[Route("rename")]
		[HttpPost]
		public HttpResponseMessage RenameDevice([FromBody] DeviceRenameRequest device)
		{
			if (string.IsNullOrWhiteSpace(device.ActualName) || string.IsNullOrWhiteSpace(device.NewName))
			{
				return Request.CreateBadRequestResponse();
			}

			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.HasDevice(user, device.NewName))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Device name already exists");
				}

				if (!database.UpdateDevice(user, device.ActualName, device.NewName))
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