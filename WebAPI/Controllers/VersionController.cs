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
	[RoutePrefix("api/v1/versions")]
	[ApiAuthentification(true)]
	public class VersionController : ApiController
	{
		[Route("all")]
		[HttpGet]
		public HttpResponseMessage Versions()
		{
			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				return Request.CreateGoodReponse(database.GetVersions(user.Id).Select(ToResponse));
			}
		}

		[Route("all/{fromVersionNumber}")]
		[HttpGet]
		public HttpResponseMessage Versions([FromUri] string fromVersionNumber)
		{
			long fromVersion;
			if (!long.TryParse(fromVersionNumber, out fromVersion))
			{
				return Request.CreateBadRequestResponse();
			}

			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				return Request.CreateGoodReponse(database.GetVersions(user.Id, fromVersion).Select(ToResponse));
			}
		}

		[Route("create")]
		[HttpPost]
		public HttpResponseMessage CreateVersion()
		{
			User user = RequestContext.GetAuthenticatedUser();
			Device device = RequestContext.GetDevice();

			using (IDatabaseService database = new DatabaseService())
			{
				Version version = database.CreateVersion(user.Id, device.Id);

				return Request.CreateGoodReponse(ToResponse(version));
			}
		}

		[Route("close/{versionNumber}")]
		[HttpPost]
		public HttpResponseMessage CloseVersion([FromUri] string versionNumber)
		{
			long version;
			if (!long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			User user = RequestContext.GetAuthenticatedUser();
			Device device = RequestContext.GetDevice();

			using (IDatabaseService database = new DatabaseService())
			{
				if (!database.HasIndiagramVersion(user.Id, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version not found");
				}

				Version v = database.CloseVersion(user.Id, device.Id, version);
				if (v == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version not found");
				}

				return Request.CreateGoodReponse(ToResponse(v));
			}
		}

		protected VersionResponse ToResponse(Version version)
		{
			return new VersionResponse(version.Number, version.Date);
		}
	}
}
