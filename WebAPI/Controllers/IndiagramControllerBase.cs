using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.Common.Responses;
using WebAPI.Filters;
using WebAPI.ProcessModels;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/collection")]
	[ApiAuthentification(true)]
	public partial class IndiagramController : ApiController
	{
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
