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
using WebAPI.Models;
using WebAPI.ProcessModels;

namespace WebAPI.Controllers
{
	public class IndiagramControllerBase : ApiController
	{
		protected async Task<HttpResponseMessage> PostFile(string id, string versionNumber, Func<IDatabaseService, IndiagramInfo, string, byte[], HttpResponseMessage> processFile)
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

				IndiagramInfo indiagramInfo = database.GetLastIndiagramInfo(user.Id, indiagramId);

				if (indiagramInfo == null)
				{
					return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Indiagram not found");
				}

				if (indiagramInfo.Version != version)
				{
					return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can not modify old indiagram version");
				}

				HttpContent file = provider.Contents.First();
				string filename = file.Headers.ContentDisposition.FileName.Trim('\"');
				byte[] buffer = await file.ReadAsByteArrayAsync();

				return processFile(database, indiagramInfo, filename, buffer);
			}
		}

		protected List<IndiagramResponse> GetCollectionTree(List<IndiagramForDevice> indiagrams)
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

			if(indexedParent.ContainsKey(-1))
			{
				return indexedParent[-1];
			}
			return new List<IndiagramResponse>();
		}

		protected IndiagramResponse ToResponse(IndiagramForDevice indiagram)
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
	}
}
