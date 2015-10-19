using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Models;
using WebAPI.ProcessModels;

namespace WebAPI.Controllers
{
	public partial class IndiagramController
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

		[Route("indiagrams/{id}")]
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

		[Route("indiagrams/{id}/{versionNumber}")]
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
				if (!database.HasIndiagramVersion(device.UserId, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version not found");
				}

				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId, version);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				return Request.CreateGoodReponse(ToResponse(resultIndiagram));
			}
		}

		[Route("images/{id}")]
		[HttpGet]
		public HttpResponseMessage Image([FromUri] string id)
		{
			throw new NotImplementedException();
		}

		[Route("images/{id}/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage Image([FromUri] string id, [FromUri] string versionNumber)
		{
			throw new NotImplementedException();
		}

		[Route("sounds/{id}")]
		[HttpGet]
		public HttpResponseMessage Sound([FromUri] string id)
		{
			throw new NotImplementedException();
		}

		[Route("sounds/{id}/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage Sound([FromUri] string id, [FromUri] string versionNumber)
		{
			throw new NotImplementedException();
		}

	}
}
