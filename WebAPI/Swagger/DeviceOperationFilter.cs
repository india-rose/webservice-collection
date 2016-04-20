using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace WebAPI.Swagger
{
	internal class DeviceOperationFilter : IOperationFilter
	{
		public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
		{
			if (operation.parameters != null && operation.parameters.Any(x => x.name == "x-indiarose-device"))
			{
				return;
			}

			if (operation.parameters == null)
			{
				operation.parameters = new List<Parameter>();
			}
			
			operation.parameters.Add(new Parameter
			{
				@in = "header",
				required = true,
				name = "x-indiarose-device",
				description = "device name",
				type = "string",
			});
		}
	}
}