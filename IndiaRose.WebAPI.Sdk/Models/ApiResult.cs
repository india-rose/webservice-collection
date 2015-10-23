namespace IndiaRose.WebAPI.Sdk.Models
{
	public class ApiResult<TStatusCode, TResult>
	{
		public TStatusCode Status { get; set; }

		public TResult Content { get; set; }

		public ApiResult()
		{
			
		}

		public ApiResult(TStatusCode status, TResult content)
		{
			Status = status;
			Content = content;
		}
	}

	public class ApiResult
	{
		public static ApiResult<TStatusCode, TResult> From<TStatusCode, TResult>(TStatusCode status, TResult content)
		{
			return new ApiResult<TStatusCode, TResult>(status, content);
		}
	}
}
