using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace WebAPI.Swagger
{
	internal class RemoveDefaultResponseOperation : IOperationFilter
	{
		public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
		{
			if (operation.responses.Count > 1)
			{
				if (operation.responses.ContainsKey("200"))
				{
					Response response = operation.responses["200"];

					if (response.schema.properties == null || response.schema.properties.Count == 0)
					{
						if (response.schema.@ref == null || response.schema.@ref == "#/definitions/Object")
						{
							operation.responses.Remove("200");
						}
					}
				}
			}
		}
	}
}