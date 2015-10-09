using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

		[Route("image/{id}/{versionNumber}")]
		[HttpPost]
		public async Task<HttpResponseMessage> PostImage([FromUri] string id, [FromUri] string versionNumber)
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				return Request.CreateBadRequestResponse();
			}

			MultipartMemoryStreamProvider provider = new MultipartMemoryStreamProvider();
			await Request.Content.ReadAsMultipartAsync(provider);

			if (provider.Contents.Count != 1)
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				User user = RequestContext.GetAuthenticatedUser();
				long indiagramId;
				long version;

				if (!long.TryParse(id, out indiagramId) || !long.TryParse(versionNumber, out version))
				{
					return Request.CreateBadRequestResponse();
				}

				if (!database.HasIndiagramVersion(user.Id, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version not found");
				}

				IndiagramInfo indiagramInfo = database.GetOrCreateIndiagramInfo(user.Id, indiagramId, version);

				if (indiagramInfo == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				HttpContent file = provider.Contents.First();
				string filename = file.Headers.ContentDisposition.FileName.Trim('\"');
				byte[] buffer = await file.ReadAsByteArrayAsync();

				IStorageService storageService = new StorageService();
				string storageFilename = storageService.UploadImage(filename, buffer);

				if (string.IsNullOrWhiteSpace(storageFilename))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Image already exists for this indiagram");
				}
				database.SetIndiagramImage(indiagramInfo, storageFilename, buffer);
			}

			return Request.CreateEmptyGoodReponse();
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
