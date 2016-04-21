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
	[RoutePrefix("api/v1/settings")]
	[ApiAuthentification(true)]
	public class SettingsController : ApiController
	{
		/// <summary>
		/// Method to get the last version of settings for a user and a device.
		/// </summary>
		/// <returns></returns>
		[Route("last")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Get the last version of setting", typeof(RequestResult<SettingsResponse>))]
		[SwaggerResponse(HttpStatusCode.NotFound, "No settings available", typeof(RequestResult))]
		public HttpResponseMessage Last()
		{
			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				Settings settings = database.GetLastSettings(device);

				if (settings == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No settings synced");
				}
				return Request.CreateGoodReponse(new SettingsResponse(settings.SerializedSettings, settings.VersionNumber, settings.Date));
			}
		}

		/// <summary>
		/// Method to update settings for a device. Used only by apps.
		/// </summary>
		/// <param name="settingsData">The new settings data.</param>
		/// <returns></returns>
		[Route("update")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Settings saved with success", typeof(RequestResult<SettingsResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing data in the request", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to save settings", typeof(RequestResult))]
		public HttpResponseMessage Update([FromBody] SettingsUpdateRequest settingsData)
		{
			if (string.IsNullOrWhiteSpace(settingsData?.Data))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				Settings settings = database.CreateSettings(device, settingsData.Data);

				if (settings == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error while saving the new settings");
				}
				return Request.CreateGoodReponse(new SettingsResponse(settings.SerializedSettings, settings.VersionNumber, settings.Date));
			}
		}

		/// <summary>
		/// Method to get specific version of settings
		/// </summary>
		/// <param name="versionNumber">The version number of settings you're interested in.</param>
		/// <returns></returns>
		[Route("get/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Return version of settings", typeof(RequestResult<SettingsResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Invalid version number", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "The settings version does not exists", typeof(RequestResult))]
		public HttpResponseMessage Get([FromUri] string versionNumber)
		{
			long version;
			if (string.IsNullOrWhiteSpace(versionNumber) || !long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				Settings settings = database.GetSettings(device, version);

				if (settings == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version of settings not found");
				}
				return Request.CreateGoodReponse(new SettingsResponse(settings.SerializedSettings, settings.VersionNumber, settings.Date));
			}
		}

		/// <summary>
		/// Get all versions of settings for a device.
		/// </summary>
		/// <returns></returns>
		[Route("all")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Return all settings version", typeof(RequestResult<List<SettingsResponse>>))]
		public HttpResponseMessage All()
		{
			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				return Request.CreateGoodReponse(database.GetSettings(device).Select(x => new SettingsResponse(x.SerializedSettings, x.VersionNumber, x.Date)).ToList());
			}
		}
	}
}