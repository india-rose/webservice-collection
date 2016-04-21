using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;
using WebAPI.Swagger;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/devices")]
	[ApiAuthentification]
	public class DeviceController : ApiController
	{
		/// <summary>
		/// Create a new device for the user. Only to use by applications.
		/// </summary>
		/// <param name="device">Device information to create.</param>
		/// <returns></returns>
		[Route("create")]
		[HttpPost]
		[SwaggerResponse(HttpStatusCode.Created, "Device to create.", null)]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Incomplete request, device name shouldn't be null or empty.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Conflict, "Device name already exists.", typeof(RequestResult))]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		public HttpResponseMessage CreateDevice([FromBody] DeviceCreateRequest device)
		{
			if (string.IsNullOrWhiteSpace(device?.Name))
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

		/// <summary>
		/// Rename a device. Used by webapp and applications.
		/// </summary>
		/// <param name="device">Information on old device name and new one.</param>
		/// <returns></returns>
		[Route("rename")]
		[SwaggerResponse(HttpStatusCode.Accepted, "Device name modification done.", null)]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Incomplete request, device name shouldn't be null or empty.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Old device name does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Conflict, "New device name already exists.", typeof(RequestResult))]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[HttpPost]
		public HttpResponseMessage RenameDevice([FromBody] DeviceRenameRequest device)
		{
			if (string.IsNullOrWhiteSpace(device?.ActualName) || string.IsNullOrWhiteSpace(device.NewName))
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

		/// <summary>
		/// Method to get the list of device for a user.
		/// </summary>
		/// <returns></returns>
		[Route("list")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[ResponseType(typeof(RequestResult<List<DeviceResponse>>))]
		[SwaggerResponse(HttpStatusCode.OK, "Get the list of device response", typeof(RequestResult<List<DeviceResponse>>))]
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