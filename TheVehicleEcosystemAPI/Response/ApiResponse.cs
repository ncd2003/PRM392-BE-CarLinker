namespace TheVehicleEcosystemAPI.Response.DTOs
{
    /// <summary>
    /// Generic API Response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(int status, string message, T? data = default)
        {
            Status = status;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Success response (200)
        /// </summary>
        public static ApiResponse<T> Success(string message, T? data = default)
        {
            return new ApiResponse<T>(200, message, data);
        }

        /// <summary>
        /// Created response (201)
        /// </summary>
        public static ApiResponse<T> Created(string message, T? data = default)
        {
            return new ApiResponse<T>(201, message, data);
        }

        /// <summary>
        /// Bad request response (400)
        /// </summary>
        public static ApiResponse<T> BadRequest(string message, T? data = default)
        {
            return new ApiResponse<T>(400, message, data);
        }

        /// <summary>
        /// Not found response (404)
        /// </summary>
        public static ApiResponse<T> NotFound(string message, T? data = default)
        {
            return new ApiResponse<T>(404, message, data);
        }

        /// <summary>
        /// Internal server error response (500)
        /// </summary>
        public static ApiResponse<T> InternalError(string message, T? data = default)
        {
            return new ApiResponse<T>(500, message, data);
        }
    }
}
