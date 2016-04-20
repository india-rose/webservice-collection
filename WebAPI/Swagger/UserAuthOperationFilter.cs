using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace WebAPI.Swagger
{
	internal class UserAuthOperationFilter : IOperationFilter
	{
		public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
		{
			if (operation.parameters != null && operation.parameters.Any(x => x.name == "x-indiarose-login"))
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
				name = "x-indiarose-login",
				description = "user login",
				type = "string",
			});

			operation.parameters.Add(new Parameter
			{
				@in = "header",
				required = true,
				name = "x-indiarose-password",
				description = "user password hashed with sha256",
                type = "string",
			});

			if (operation.responses == null)
			{
				operation.responses = new Dictionary<string, Response>();
			}

			if (!operation.responses.ContainsKey("401"))
			{
				operation.responses.Add("401", new Response
				{
					description = "Invalid credentials or device name (if requested)"
				});
			}
		}
	}
}