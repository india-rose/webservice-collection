namespace WebAPI.Helpers
{
	public class RequestResult
	{
		public bool HasError { get; set; }

		public int ErrorCode { get; set; }

		public string ErrorMessage { get; set; }
	}

	public class RequestResult<T> : RequestResult
	{
		public T Content { get; set; }
	}
}
