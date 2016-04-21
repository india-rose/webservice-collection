using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Models;
using WebAPI.ProcessModels;
using WebAPI.Swagger;

namespace WebAPI.Controllers
{
	public partial class IndiagramController
	{
		/// <summary>
		/// Update or create an indiagram.
		/// </summary>
		/// <remarks>Set the id to number &lt; 0 if you want to create a new indiagram.</remarks>
		/// <param name="request">Indiagram data.</param>
		/// <returns></returns>
		[Route("indiagrams/update")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "The updated or created indiagram.", typeof(RequestResult<IndiagramResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Version or indiagram not found.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Forbidden, "Can not update this version", typeof(RequestResult))]
		public HttpResponseMessage UpdateIndiagram([FromBody] IndiagramRequest request)
		{
			if (string.IsNullOrWhiteSpace(request?.Text))
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

		/// <summary>
		/// Update multiple indiagram at the same time.
		/// </summary>
		/// <remarks>The returned response map all sent ids to database ids.</remarks>
		/// <param name="request">Data of the update.</param>
		/// <returns></returns>
		[Route("indiagrams/updates")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Update and creation done.", typeof(RequestResult<List<MappedIndiagramResponse>>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields or invalid request.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Version or indiagram not found.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Forbidden, "Can not update this version", typeof(RequestResult))]
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

		/// <summary>
		/// Update image for an indiagram.
		/// </summary>
		/// <param name="id">The indiagram id.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <param name="fileRequest">Image data.</param>
		/// <returns></returns>
		[Route("images/{id}/{versionNumber}")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Upload done.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Version or indiagram not found.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Forbidden, "Can not update this version", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Conflict, "Image has already been updated for this version", typeof(RequestResult))]
		public HttpResponseMessage PostImage([FromUri] string id, [FromUri] string versionNumber, [FromBody] FileUploadRequest fileRequest)
		{
			long indiagramId;
			long version;

			if (!long.TryParse(id, out indiagramId) || !long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			if (string.IsNullOrEmpty(fileRequest?.Filename) || fileRequest.Content.Length == 0)
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				User user = RequestContext.GetAuthenticatedUser();
				Device device = RequestContext.GetDevice();
				
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

		/// <summary>
		/// Update sound for an indiagram.
		/// </summary>
		/// <param name="id">The indiagram id.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <param name="fileRequest">Sound data.</param>
		/// <returns></returns>
		[Route("sounds/{id}/{versionNumber}")]
		[HttpPost]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Upload done.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Version or indiagram not found.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Forbidden, "Can not update this version", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Conflict, "Sound has already been updated for this version", typeof(RequestResult))]
		public HttpResponseMessage PostSound([FromUri] string id, [FromUri] string versionNumber, [FromBody] FileUploadRequest fileRequest)
		{
			long indiagramId;
			long version;

			if (!long.TryParse(id, out indiagramId) || !long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			if (string.IsNullOrEmpty(fileRequest?.Filename) || fileRequest.Content.Length == 0)
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				User user = RequestContext.GetAuthenticatedUser();
				Device device = RequestContext.GetDevice();
				
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
