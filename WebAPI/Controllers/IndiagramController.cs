using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using WebAPI.Common.Responses;
using WebAPI.Database;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Models;
using WebAPI.ProcessModels;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/indiagrams")]
	[ApiAuthentification(true)]
	public class IndiagramController : ApiController
	{
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

		private List<IndiagramResponse> GetCollectionTree(List<IndiagramForDevice> indiagrams)
		{
			//List<IndiagramResponse> topLevel = indiagrams.Where(x => x.ParentId < 0).Select(ToResponse).ToList();
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
				Text = indiagram.Text
			};
		}
	}
}
