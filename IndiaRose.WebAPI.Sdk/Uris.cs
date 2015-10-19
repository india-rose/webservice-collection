namespace IndiaRose.WebAPI.Sdk
{
	static class Uris
	{
		private const string PREFIX = "/api/v1";

		// users
		private const string USER_PREFIX = PREFIX + "/users";
		public const string USER_REGISTER = USER_PREFIX + "/register";
		public const string USER_LOGIN = USER_PREFIX + "/login";

		// devices
		private const string DEVICE_PREFIX = PREFIX + "/devices";
		public const string DEVICE_CREATE = DEVICE_PREFIX + "/create";
		public const string DEVICE_RENAME = DEVICE_PREFIX + "/rename";
		public const string DEVICE_LIST = DEVICE_PREFIX + "/list";

		// settings
		private const string SETTINGS_PREFIX = PREFIX + "/settings";
		public const string SETTINGS_LAST = SETTINGS_PREFIX + "/last";
		public const string SETTINGS_UPDATE = SETTINGS_PREFIX + "/update";
		public const string SETTINGS_GET_VERSION = SETTINGS_PREFIX + "/get/{0}";
		public const string SETTINGS_LIST = SETTINGS_PREFIX + "/all";

		// versions
		private const string VERSIONS_PREFIX = PREFIX + "/versions";
		public const string VERSIONS_ALL = VERSIONS_PREFIX + "/all";
		public const string VERSIONS_ALL_FROM = VERSIONS_PREFIX + "/all/{0}";
		public const string VERSIONS_CREATE = VERSIONS_PREFIX + "/create";
	}
}
