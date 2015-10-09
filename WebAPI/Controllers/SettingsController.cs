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
	[RoutePrefix("/api/v1/settings")]
	[ApiAuthentification(true)]
	public class SettingsController : ApiController
	{
		[Route("last")]
		[HttpGet]
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

		[Route("update")]
		[HttpPost]
		public HttpResponseMessage Update([FromBody] string settingsData)
		{
			if (string.IsNullOrWhiteSpace(settingsData))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				Settings settings = database.CreateSettings(device, settingsData);

				if (settings == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error while saving the new settings");
				}
				return Request.CreateGoodReponse(new SettingsResponse(settings.SerializedSettings, settings.VersionNumber, settings.Date));
			}
		}

		[Route("get/{versionNumber}")]
		[HttpGet]
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

		[Route("all")]
		[HttpGet]
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