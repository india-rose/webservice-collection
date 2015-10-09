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
	public class IndiagramController : ApiController
	{
		#region HttpGet

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
			throw new NotImplementedException();
		}

		[Route("indiagram/{id}/{versionNumber}")]
		[HttpGet]
		public HttpResponseMessage Indiagram([FromUri] string id, [FromUri] string versionNumber)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		[Route("versions/{fromVersionNumber}")]
		[HttpGet]
		public HttpResponseMessage Versions([FromUri] string fromVersionNumber)
		{
			throw new NotImplementedException();
		}

		#endregion

		[Route("create/version")]
		[HttpPost]
		public HttpResponseMessage CreateVersion()
		{
			throw new NotImplementedException();
		}

		[Route("create/indiagram")]
		[HttpPost]
		public HttpResponseMessage CreateIndiagram([FromBody] string indiagramText)
		{
			throw new NotImplementedException();
		}

		private List<IndiagramResponse> GetCollectionTree(List<IndiagramForDevice> indiagrams)
		{
			Dictionary<long, IndiagramResponse> indexedCategories = new Dictionary<long, IndiagramResponse>();
			Dictionary<long, List<IndiagramResponse>> indexedParent = new Dictionary<long, List<IndiagramResponse>>();

			foreach (IndiagramForDevice indiagram in indiagrams)
			{
				IndiagramResponse r = ToResponse(indiagram);
				if (indiagram.IsCategory)
				{
					indexedCategories.Add(r.DatabaseId, r);
				}

				if (!indexedParent.ContainsKey(indiagram.ParentId))
				{
					indexedParent.Add(indiagram.ParentId, new List<IndiagramResponse>());
				}
				indexedParent[indiagram.ParentId].Add(r);
			}

			foreach (KeyValuePair<long, List<IndiagramResponse>> categoryContent in indexedParent.Where(x => x.Key > 0))
			{
				indexedCategories[categoryContent.Key].Children = categoryContent.Value;
			}

			return indexedParent[-1];
		}

		private IndiagramResponse ToResponse(IndiagramForDevice indiagram)
		{
			return new IndiagramResponse
			{
				DatabaseId = indiagram.Id,
				IsEnabled = indiagram.IsEnabled,
				ImagePath = indiagram.ImagePath,
				IsCategory = indiagram.IsCategory,
				SoundPath = indiagram.SoundPath,
				Text = indiagram.Text,
				Position = indiagram.Position
			};
		}
	}
}
