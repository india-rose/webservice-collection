using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;
using WebAPI.ProcessModels;
using Version = WebAPI.Models.Version;

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

		#endregion

		[Route("create/version")]
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

		[Route("update/indiagram")]
		[HttpPost]
		public HttpResponseMessage CreateIndiagram([FromBody] IndiagramRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Text))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				//TODO : call create only if request.Id < 0
				Device device = RequestContext.GetDevice();
				if (request.Id < 0)
				{
					Indiagram indiagram = database.CreateIndiagram(device.UserId, device.Id, request);
					return Request.CreateGoodReponse(database.GetIndiagram(device, indiagram.Id));
				}
				else
				{
					//TODO
					throw new NotImplementedException();
				}
			}
		}

		[Route("image/{id}/{versionNumber}")]
		[HttpPost]
		public async Task<HttpResponseMessage> PostImage([FromUri] string id, [FromUri] string versionNumber)
		{
			return await PostFile(id, versionNumber, (database, indiagramInfo, filename, buffer) =>
			{
				IStorageService storageService = new StorageService();
				if (!storageService.UploadImage(indiagramInfo, buffer))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Indiagram image already exists and can't be replaced");
				}

				database.SetIndiagramImage(indiagramInfo, filename, buffer);
				return Request.CreateEmptyGoodReponse();
			});
		}

		[Route("sound/{id}/{versionNumber}")]
		[HttpPost]
		public async Task<HttpResponseMessage> PostSound([FromUri] string id, [FromUri] string versionNumber)
		{
			return await PostFile(id, versionNumber, (database, indiagramInfo, filename, buffer) =>
			{
				IStorageService storageService = new StorageService();
				if (!storageService.UploadSound(indiagramInfo, buffer))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Indiagram sound already exists and can't be replaced");
				}

				database.SetIndiagramSound(indiagramInfo, filename, buffer);
				return Request.CreateEmptyGoodReponse();
			});
		}

		private async Task<HttpResponseMessage> PostFile(string id, string versionNumber, Func<IDatabaseService, IndiagramInfo, string, byte[], HttpResponseMessage> processFile)
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

				return processFile(database, indiagramInfo, filename, buffer);
			}
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
				ImageHash = indiagram.ImageHash,
				IsCategory = indiagram.IsCategory,
				SoundHash = indiagram.SoundHash,
				Text = indiagram.Text,
				Position = indiagram.Position
			};
		}

		private VersionResponse ToResponse(Version version)
		{
			return new VersionResponse(version.Number, version.Date);
		}
	}
}
