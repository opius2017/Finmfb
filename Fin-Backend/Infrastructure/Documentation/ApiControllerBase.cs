using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;

namespace Fin_Backend.Infrastructure.Documentation
{
    /// <summary>
    /// Base API controller with standard response handling and documentation
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error occurred")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "The request was invalid")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication is required")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, "The user does not have the required permissions")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Creates a successful response with data
        /// </summary>
        /// <typeparam name="T">Type of data being returned</typeparam>
        /// <param name="data">Data to return</param>
        /// <returns>An ActionResult with the data</returns>
        protected ActionResult<ApiResponse<T>> Success<T>(T data)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = null,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Creates a successful response with a message
        /// </summary>
        /// <param name="message">Message to return</param>
        /// <returns>An ActionResult with the message</returns>
        protected ActionResult<ApiResponse<object>> Success(string message)
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = null,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>An ActionResult with the error</returns>
        protected ActionResult<ApiResponse<object>> Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            return StatusCode((int)statusCode, response);
        }
    }

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Data returned from the API
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Message to the client
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}