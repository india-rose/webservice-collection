using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Common.Requests;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Models;

namespace WebAPI.Controllers
{
	public partial class IndiagramController
	{
		[Route("indiagrams/update")]
		[HttpPost]
		public HttpResponseMessage CreateIndiagram([FromBody] IndiagramRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Text))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();
				if (!database.HasIndiagramVersion(device.UserId, request.Version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "version not found");
				}

				if (request.Id < 0)
				{
					Indiagram indiagram = database.CreateIndiagram(device.UserId, device.Id, request);
					return Request.CreateGoodReponse(ToResponse(database.GetIndiagram(device, indiagram.Id)));
				}
				else
				{
					//update
					Indiagram indiagram = database.UpdateIndiagram(device.UserId, device.Id, request);

					if (indiagram == null)
					{
						return Request.CreateErrorResponse(HttpStatusCode.NotFound, "indiagram not found");
					}

					return Request.CreateGoodReponse(ToResponse(database.GetIndiagram(device, indiagram.Id)));
				}
				
			}
		}

		[Route("images/{id}/{versionNumber}")]
		[HttpPost]
		public HttpResponseMessage PostImage([FromUri] string id, [FromUri] string versionNumber, [FromBody] FileUploadRequest fileRequest)
		{
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

				IndiagramInfo indiagramInfo = database.GetLastIndiagramInfo(user.Id, indiagramId);

				if (indiagramInfo == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (indiagramInfo.Version != version)
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not modify old indiagram version");
				}

				string filename = fileRequest.Filename;
				byte[] buffer = fileRequest.Content;

				IStorageService storageService = new StorageService();
				if (!storageService.UploadImage(indiagramInfo, buffer))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Indiagram image already exists and can't be replaced");
				}

				database.SetIndiagramImage(indiagramInfo, filename, buffer);
				return Request.CreateEmptyGoodReponse();
			}
		}

		[Route("sounds/{id}/{versionNumber}")]
		[HttpPost]
		public HttpResponseMessage PostSound([FromUri] string id, [FromUri] string versionNumber, [FromBody] FileUploadRequest fileRequest)
		{
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

				IndiagramInfo indiagramInfo = database.GetLastIndiagramInfo(user.Id, indiagramId);

				if (indiagramInfo == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (indiagramInfo.Version != version)
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not modify old indiagram version");
				}

				string filename = fileRequest.Filename;
				byte[] buffer = fileRequest.Content;

				IStorageService storageService = new StorageService();
				if (!storageService.UploadSound(indiagramInfo, buffer))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Indiagram sound already exists and can't be replaced");
				}

				database.SetIndiagramSound(indiagramInfo, filename, buffer);
				return Request.CreateEmptyGoodReponse();
			}
		}
	}
}
