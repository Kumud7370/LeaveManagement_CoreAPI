using System;
using System.Collections.Generic;

namespace AttendanceManagementSystem.Models.DTOs.Common
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<ErrorDetail>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }

        public static ApiResponseDto<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponseDto<T> ErrorResponse(string message, List<ErrorDetail>? errors = null)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    public class ErrorDetail
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}