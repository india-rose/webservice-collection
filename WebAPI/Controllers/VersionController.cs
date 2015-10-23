using System.Linq;
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

			using (IDatabaseService database = new DatabaseService())
			{
				Version version = database.CreateVersion(user.Id);

				return Request.CreateGoodReponse(ToResponse(version));
			}
		}

		protected VersionResponse ToResponse(Version version)
		{
			return new VersionResponse(version.Number, version.Date);
		}
	}
}
