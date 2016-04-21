using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;
using WebAPI.Swagger;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/versions")]
	[ApiAuthentification(true)]
	public class VersionController : ApiController
	{
		/// <summary>
		/// List all versions available for the collection.
		/// </summary>
		/// <returns></returns>
		[Route("all")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "List of all versions", typeof(RequestResult<List<VersionResponse>>))]
		public HttpResponseMessage Versions()
		{
			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				return Request.CreateGoodReponse(database.GetVersions(user.Id).Select(ToResponse));
			}
		}

		/// <summary>
		/// List all version from a specific number to the last one.
		/// </summary>
		/// <param name="fromVersionNumber">The minimal version.</param>
		/// <returns></returns>
		[Route("all/{fromVersionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "List of all versions", typeof(RequestResult<List<VersionResponse>>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields", typeof(RequestResult))]
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

		/// <summary>
		/// Create a new version for the collection.
		/// </summary>
		/// <returns></returns>
		[Route("create")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Get the new version", typeof(RequestResult<VersionResponse>))]
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

		/// <summary>
		/// Close a version to commit all modifications.
		/// </summary>
		/// <remarks>You'll be no longer able to save modifications for this version.</remarks>
		/// <param name="versionNumber">The version to close.</param>
		/// <returns></returns>
		[Route("close/{versionNumber}")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "List of all versions", typeof(RequestResult<List<VersionResponse>>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields", typeof(RequestResult))]
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
