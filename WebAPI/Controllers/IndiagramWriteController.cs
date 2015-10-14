using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Common.Requests;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/collection")]
	[ApiAuthentification(true)]
	public class IndiagramWriteController : IndiagramControllerBase
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

		[Route("sounds/{id}/{versionNumber}")]
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
	}
}
