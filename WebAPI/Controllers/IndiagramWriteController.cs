using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Models;
using WebAPI.ProcessModels;

namespace WebAPI.Controllers
{
	public partial class IndiagramController
	{
		[Route("indiagrams/update")]
		[HttpPost]
		public HttpResponseMessage UpdateIndiagram([FromBody] IndiagramRequest request)
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

				if (!database.CanPushInVersion(device.UserId, device.Id, request.Version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not update in this version, version is closed or didn't created by this device");
				}

				if (request.Id < 0)
				{
					IndiagramForDevice indiagram = database.CreateIndiagram(device.UserId, device.Id, request);
					return Request.CreateGoodReponse(ToResponse(indiagram));
				}
				else
				{
					//update
					IndiagramForDevice indiagram = database.UpdateIndiagram(device.UserId, device.Id, request);

					if (indiagram == null)
					{
						return Request.CreateErrorResponse(HttpStatusCode.NotFound, "indiagram not found");
					}

					return Request.CreateGoodReponse(ToResponse(indiagram));
				}
				
			}
		}

		[Route("indiagrams/updates")]
		public HttpResponseMessage UpdateIndiagrams([FromBody] List<IndiagramRequest> request)
		{
			if (request == null || request.Any(x => string.IsNullOrEmpty(x.Text)))
			{
				return Request.CreateBadRequestResponse();
			}

			if (request.Count == 0)
			{
				return Request.CreateGoodReponse(new List<MappedIndiagramResponse>());
			}

			long version = request.First().Version;
			if (request.Any(x => x.Version != version))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				Device device = RequestContext.GetDevice();

				if (!database.HasIndiagramVersion(device.UserId, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "version not found");
				}

				if (!database.CanPushInVersion(device.UserId, device.Id, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not update in this version, version is closed or didn't created by this device");
				}

				bool hasError = false;
				Queue<IndiagramRequest> input = new Queue<IndiagramRequest>(request);
				List<IndiagramRequest> orderedRequests = new List<IndiagramRequest>();


				int noChangeCount = 0;
				Dictionary<long, long> mappedIds = new Dictionary<long, long>();
				while (input.Count > 0)
				{
					IndiagramRequest item = input.Dequeue();
					if (item.ParentId >= -1 || mappedIds.ContainsKey(item.ParentId))
					{
						noChangeCount = 0;
						if (item.Id < -1)
						{
							mappedIds.Add(item.Id, 0);
						}
						orderedRequests.Add(item);
					}
					else
					{
						noChangeCount++;
						if (noChangeCount == input.Count)
						{
							return Request.CreateBadRequestResponse("Cycle/missing indiagrams detected in collection");
						}
						input.Enqueue(item);
					}
				}

				List<MappedIndiagramResponse> result = orderedRequests.Select(x =>
				{
					IndiagramForDevice indiagram;

					if (x.ParentId < 0)
					{
						x.ParentId = mappedIds[x.ParentId];
					}

					if (x.Id < 0)
					{
						indiagram = database.CreateIndiagram(device.UserId, device.Id, x);

						mappedIds[x.Id] = indiagram.Id;
					}
					else
					{
						indiagram = database.UpdateIndiagram(device.UserId, device.Id, x);
					}
					if (indiagram == null)
					{
						hasError = true;
						return null;
					}
					return new MappedIndiagramResponse
					{
						SentId = x.Id,
						DatabaseId = indiagram.Id,
						ParentId = indiagram.ParentId
					};
				}).ToList();

				if (hasError)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "indiagram not found");
				}
				return Request.CreateGoodReponse(result);
			}
		}

		[Route("images/{id}/{versionNumber}")]
		[HttpPost]
		public HttpResponseMessage PostImage([FromUri] string id, [FromUri] string versionNumber, [FromBody] FileUploadRequest fileRequest)
		{
			using (IDatabaseService database = new DatabaseService())
			{
				User user = RequestContext.GetAuthenticatedUser();
				Device device = RequestContext.GetDevice();
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

				if (!database.CanPushInVersion(device.UserId, device.Id, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not update in this version, version is closed or didn't created by this device");
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
				Device device = RequestContext.GetDevice();
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

				if (!database.CanPushInVersion(device.UserId, device.Id, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not update in this version, version is closed or didn't created by this device");
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
