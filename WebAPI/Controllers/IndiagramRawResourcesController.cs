using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
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
		/// Get last version of the image associated to an indiagram.
		/// </summary>
		/// <param name="id">Indiagram id.</param>
		/// <returns></returns>
		[Route("rawimages/{id}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Image of the indiagram.", typeof(byte[]))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram or image does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage RawImage([FromUri] string id)
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

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(content)
				};

				result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeMapping.GetMimeMapping(resultIndiagram.ImageFile));
				return result;
			}
		}

		/// <summary>
		/// Get image of the version of the indiagram.
		/// </summary>
		/// <param name="id">The id of the indiagram.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <returns></returns>
		[Route("rawimages/{id}/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Image of the indiagram.", typeof(byte[]))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram, version or image does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage RawImage([FromUri] string id, [FromUri] string versionNumber)
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

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(content)
				};

				result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeMapping.GetMimeMapping(resultIndiagram.ImageFile));
				return result;
			}
		}

		/// <summary>
		/// Get the sounds of the last version of the indiagram.
		/// </summary>
		/// <param name="id">The id of the indiagram.</param>
		/// <returns></returns>
		[Route("rawsounds/{id}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Sound of the indiagram.", typeof(byte[]))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram or sound does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage RawSound([FromUri] string id)
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

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(content)
				};
				
				result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeMapping.GetMimeMapping(resultIndiagram.SoundFile));
				return result;
			}
		}

		/// <summary>
		/// Get the sound of the indiagram in a specific version.
		/// </summary>
		/// <param name="id">The id of the indiagram.</param>
		/// <param name="versionNumber">The version number.</param>
		/// <returns></returns>
		[Route("rawsounds/{id}/{versionNumber}")]
		[HttpGet]
		[SwaggerOperationFilter(typeof(UserAuthOperationFilter))]
		[SwaggerOperationFilter(typeof(DeviceOperationFilter))]
		[SwaggerResponse(HttpStatusCode.OK, "Sound of the indiagram.", typeof(byte[]))]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Missing fields.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.NotFound, "Indiagram, version or sound does not exists.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.InternalServerError, "Unable to contact storage server.", typeof(RequestResult))]
		public HttpResponseMessage RawSound([FromUri] string id, [FromUri] string versionNumber)
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

				HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(content)
				};

				result.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeMapping.GetMimeMapping(resultIndiagram.SoundFile));
				return result;
			}
		}
	}
}