namespace IndiaRose.WebAPI.Sdk
{
	static class Uris
	{
		private const string PREFIX = "/api/v1";

		public const string ALIVE = PREFIX + "/alive";

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

		// collections
		private const string COLLECTION_PREFIX = PREFIX + "/collection";
		public const string COLLECTION_ALL = COLLECTION_PREFIX + "/all";
		public const string COLLECTION_ALL_IN_VERSION = COLLECTION_PREFIX + "/all/{0}";
		public const string INDIAGRAM_GET = COLLECTION_PREFIX + "/indiagrams/{0}";
		public const string INDIAGRAM_GET_IN_VERSION = COLLECTION_PREFIX + "/indiagrams/{0}/{1}";
		public const string IMAGE_GET = COLLECTION_PREFIX + "/images/{0}";
		public const string IMAGE_GET_IN_VERSION = COLLECTION_PREFIX + "/images/{0}/{1}";
		public const string SOUND_GET = COLLECTION_PREFIX + "/sounds/{0}";
		public const string SOUND_GET_IN_VERSION = COLLECTION_PREFIX + "/sounds/{0}/{1}";
		public const string INDIAGRAM_UPDATE = COLLECTION_PREFIX + "/indiagrams/update";
		public const string IMAGE_UPLOAD = COLLECTION_PREFIX + "/images/{0}/{1}";
		public const string SOUND_UPLOAD = COLLECTION_PREFIX + "/sounds/{0}/{1}";
	}
}
