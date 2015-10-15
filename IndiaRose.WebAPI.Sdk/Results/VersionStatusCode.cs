using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndiaRose.WebAPI.Sdk.Results
{
	public enum VersionStatusCode
	{
		InternalError,
		BadRequest,
		UnknownError,
		SettingsNotFound,
		InvalidLoginOrPassword,
		Ok
	}
}
