using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;
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
		/// Get last version of the complete collection.
		/// </summary>
		/// <returns></returns>
		[Route("all")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Get the last version of the collection.", typeof(RequestResult<List<IndiagramResponse>>))]
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

		/// <summary>
		/// Get a specific version of the complete collection.
		/// </summary>
		/// <param name="versionNumber">The version.</param>
		/// <returns></returns>
		[Route("all/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Get a specific version of the collection.", typeof(RequestResult<List<IndiagramResponse>>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Version does not exists.", typeof(RequestResult))]
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
				if (database.HasIndiagramVersion(device.UserId, version) && !database.IsVersionOpen(device.UserId, version))
				{
					List<IndiagramForDevice> collection = database.GetIndiagrams(device, version);
					List<IndiagramResponse> indiagrams = GetCollectionTree(collection);

					return Request.CreateGoodReponse(indiagrams);
				}

				return Request.CreateErrorResponse(HttpStatusCode.NotFound, "indiagram version not found");
			}
		}

		/// <summary>
		/// Get last version of an indiagram.
		/// </summary>
		/// <param name="id">Id of the indiagram.</param>
		/// <returns></returns>
		[Route("indiagrams/{id}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Version of the indiagram.", typeof(RequestResult<IndiagramResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram does not exists.", typeof(RequestResult))]
		public HttpResponseMessage Indiagram([FromUri] string id)
		{
			long indiagramId;
			if (!long.TryParse(id, out indiagramId))
			{
				return Request.CreateBadRequestResponse();
			}

			Device device = RequestContext.GetDevice();
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

		/// <summary>
		/// Get specific version of an indiagram.
		/// </summary>
		/// <param name="id">The indiagram id.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <returns></returns>
		[Route("indiagrams/{id}/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Version of the indiagram.", typeof(RequestResult<IndiagramResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram or version does not exists.", typeof(RequestResult))]
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
				if (!database.HasIndiagramVersion(device.UserId, version) || database.IsVersionOpen(device.UserId, version))
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

		/// <summary>
		/// Get last version of the image associated to an indiagram.
		/// </summary>
		/// <param name="id">Indiagram id.</param>
		/// <returns></returns>
		[Route("images/{id}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Image of the indiagram.", typeof(RequestResult<FileDownloadResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram or image does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage Image([FromUri] string id)
		{
			long indiagramId;
			if (!long.TryParse(id, out indiagramId))
			{
				return Request.CreateBadRequestResponse();
			}

			Device device = RequestContext.GetDevice();
			using (IDatabaseService database = new DatabaseService())
			{
				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (string.IsNullOrWhiteSpace(resultIndiagram.ImageHash))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No image file");
				}

				IStorageService storageService = new StorageService();
				byte[] content = storageService.DownloadImage(resultIndiagram.Id, resultIndiagram.Version);

				if (content == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error while downloading file");
				}

				return Request.CreateGoodReponse(new FileDownloadResponse
				{
					FileName = resultIndiagram.ImageFile,
					Content = content
				});
			}
		}

		/// <summary>
		/// Get image of the version of the indiagram.
		/// </summary>
		/// <param name="id">The id of the indiagram.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <returns></returns>
		[Route("images/{id}/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Image of the indiagram.", typeof(RequestResult<FileDownloadResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram, version or image does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage Image([FromUri] string id, [FromUri] string versionNumber)
		{
			long indiagramId;
			long version;
			if (!long.TryParse(id, out indiagramId) || !long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			Device device = RequestContext.GetDevice();
			using (IDatabaseService database = new DatabaseService())
			{
				if (!database.HasIndiagramVersion(device.UserId, version) || database.IsVersionOpen(device.UserId, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version not found");
				}

				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId, version);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (string.IsNullOrWhiteSpace(resultIndiagram.ImageHash))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No image file");
				}

				IStorageService storageService = new StorageService();
				byte[] content = storageService.DownloadImage(resultIndiagram.Id, resultIndiagram.Version);

				if (content == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error while downloading file");
				}

				return Request.CreateGoodReponse(new FileDownloadResponse
				{
					FileName = resultIndiagram.ImageFile,
					Content = content
				});
			}
		}

		/// <summary>
		/// Get the sounds of the last version of the indiagram.
		/// </summary>
		/// <param name="id">The id of the indiagram.</param>
		/// <returns></returns>
		[Route("sounds/{id}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Sound of the indiagram.", typeof(RequestResult<FileDownloadResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram or sound does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage Sound([FromUri] string id)
		{
			long indiagramId;
			if (!long.TryParse(id, out indiagramId))
			{
				return Request.CreateBadRequestResponse();
			}

			Device device = RequestContext.GetDevice();
			using (IDatabaseService database = new DatabaseService())
			{
				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (string.IsNullOrWhiteSpace(resultIndiagram.SoundHash))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No sound file");
				}

				IStorageService storageService = new StorageService();
				byte[] content = storageService.DownloadSound(resultIndiagram.Id, resultIndiagram.Version);

				if (content == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error while downloading file");
				}

				return Request.CreateGoodReponse(new FileDownloadResponse
				{
					FileName = resultIndiagram.ImageFile,
					Content = content
				});
			}
		}

		/// <summary>
		/// Get the sound of the indiagram in a specific version.
		/// </summary>
		/// <param name="id">The id of the indiagram.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <returns></returns>
		[Route("sounds/{id}/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Sound of the indiagram.", typeof(RequestResult<FileDownloadResponse>))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram, version or sound does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage Sound([FromUri] string id, [FromUri] string versionNumber)
		{
			long indiagramId;
			long version;
			if (!long.TryParse(id, out indiagramId) || !long.TryParse(versionNumber, out version))
			{
				return Request.CreateBadRequestResponse();
			}

			Device device = RequestContext.GetDevice();
			using (IDatabaseService database = new DatabaseService())
			{
				if (!database.HasIndiagramVersion(device.UserId, version) || database.IsVersionOpen(device.UserId, version))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Version not found");
				}

				IndiagramForDevice resultIndiagram = database.GetIndiagram(device, indiagramId, version);

				if (resultIndiagram == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (string.IsNullOrWhiteSpace(resultIndiagram.SoundHash))
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No sound file");
				}

				IStorageService storageService = new StorageService();
				byte[] content = storageService.DownloadSound(resultIndiagram.Id, resultIndiagram.Version);

				if (content == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error while downloading file");
				}

				return Request.CreateGoodReponse(new FileDownloadResponse
				{
					FileName = resultIndiagram.ImageFile,
					Content = content
				});
			}
		}
	}
}
