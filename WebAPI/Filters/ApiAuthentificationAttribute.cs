using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using WebAPI.Database;
using WebAPI.Extensions;

#pragma warning disable 1998
namespace WebAPI.Filters
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
	internal sealed class ApiAuthentificationAttribute : Attribute, IAuthenticationFilter
	{
		private readonly bool _deviceRequired;

		public ApiAuthentificationAttribute(bool deviceRequired = false)
		{
			_deviceRequired = deviceRequired;
		}

		public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			HttpRequestMessage request = context.Request;

			string login = request.GetHeaderValue("x-indiarose-login");
			string password = request.GetHeaderValue("x-indiarose-password");

			if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
			{
				context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
				return;
			}

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.CheckAuthentification(login, password))
				{
					AuthentificationPrincipal identity = new AuthentificationPrincipal(database.GetUserByLogin(login), password);

					string deviceName = request.GetHeaderValue("x-indiarose-device");
					if (!string.IsNullOrWhiteSpace(deviceName))
					{
						identity.Device = database.GetDevice(identity.User, deviceName);

						if (identity.Device == null && _deviceRequired)
						{
							context.ErrorResult = new AuthenticationFailureResult("Invalid device name", request);
							return;
						}
					}
					else if (_deviceRequired)
					{
						context.ErrorResult = new AuthenticationFailureResult("Missing device name", request);
						return;
					}

					context.Principal = identity;
				}
				else
				{
					context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
				}
			}
		}

		public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			//no challenge
		}

		public bool AllowMultiple { get { return false; } }
	}
}
