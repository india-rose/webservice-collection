using System;
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
using WebAPI.ProcessModels;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/collection")]
	[ApiAuthentification(true)]
	public class IndiagramReadController : IndiagramControllerBase
	{
		[Route("all")]
		[HttpGet]
		public HttpResponseMessage All()
		{
			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();

				List<IndiagramForDevice> collection = database.GetIndiagrams(device);
				List<IndiagramResponse> indiagrams = GetCollectionTree(collection);

				return Request.CreateGoodReponse(indiagrams);
			}
		}

		[Route("all/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage All([FromUri] string versionNumber)
		{
			long version;
			if (!long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				if (database.HasIndiagramVersion(device.UserId, version))
				{

					List<IndiagramForDevice> collection = database.GetIndiagrams(device, version);
					List<IndiagramResponse> indiagrams = GetCollectionTree(collection);

					return Request.CreateGoodReponse(indiagrams);
				}

				return Request.CreateErrorResponse(HttpStatusCode.NotFound, "indiagram version not found");
			}
		}

		[Route("indiagram/{id}")]
		[HttpGet]
		public HttpResponseMessage Indiagram([FromUri] string id)
		{
			long indiagramId;
			Device device = RequestContext.GetDevice();
			if (!long.TryParse(id, out indiagramId))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				return Request.CreateGoodReponse(ToResponse(resultIndiagram));
			}
		}

		[Route("indiagram/{id}/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage Indiagram([FromUri] string id, [FromUri] string versionNumber)
		{
			long indiagramId;
			long version;
			Device device = RequestContext.GetDevice();
			if (!long.TryParse(id, out indiagramId) || !long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId, version);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				return Request.CreateGoodReponse(ToResponse(resultIndiagram));
			}
		}

		[Route("image/{id}")]
		[HttpGet]
		public HttpResponseMessage Image([FromUri] string id)
		{
			throw new NotImplementedException();
		}

		[Route("image/{id}/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage Image([FromUri] string id, [FromUri] string versionNumber)
		{
			throw new NotImplementedException();
		}

		[Route("sound/{id}")]
		[HttpGet]
		public HttpResponseMessage Sound([FromUri] string id)
		{
			throw new NotImplementedException();
		}

		[Route("sound/{id}/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage Sound([FromUri] string id, [FromUri] string versionNumber)
		{
			throw new NotImplementedException();
		}

		[Route("versions")]
		[HttpGet]
		public HttpResponseMessage Versions()
		{
			User user = RequestContext.GetAuthenticatedUser();

			using (IDatabaseService database = new DatabaseService())
			{
				return Request.CreateGoodReponse(database.GetVersions(user.Id).Select(x => ToResponse(x)));
			}
		}

		[Route("versions/{fromVersionNumber}")]
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
				return Request.CreateGoodReponse(database.GetVersions(user.Id).Select(x => ToResponse(x)));
			}
		}
	}
}
